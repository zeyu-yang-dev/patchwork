using Godot;
using Patchwork.Domain;

namespace Patchwork.Scenes.PatchView;

public partial class PatchView : Control
{
    public const int MatrixSize = 5;
    public const float CellSize = 50.0f;
    
    public static readonly Vector2 ViewSize = new(MatrixSize * CellSize, MatrixSize * CellSize);
    public static readonly Vector2 CenterOffset = ViewSize / 2.0f;
    private const string OriginalTextureDirectory = "res://Assets/Patches/original";
    private const string MirroredTextureDirectory = "res://Assets/Patches/mirrored";

    public Patch Patch { get; private set; }
    public PlacedPatch PlacedPatch { get; private set; }

    public (int col, int row)? Coordinate => PlacedPatch?.Coordinate;

    public override void _Ready()
    {
        CustomMinimumSize = ViewSize;
        Size = ViewSize;
        PivotOffset = CenterOffset;
        MouseFilter = MouseFilterEnum.Pass;
    }

    // 当这个节点需要重绘时，Godot 会调用它
    public override void _Draw()
    {
        var texture = GetCurrentTexture();

        if (texture == null)
        {
            return;
        }

        // 如果没有这句，旋转通常会围绕左上角发生，就不对了
        DrawSetTransform(CenterOffset, GetCurrentRotationRadians(), Vector2.One);
        // 真正把图片画出来，因为前面已经把坐标原点移到了中心 (25,25)，所以这里再从 (-25,-25) 开始画一个 50x50 的图
        DrawTextureRect(texture, new Rect2(-CenterOffset, ViewSize), false);
        // 恢复默认变换
        DrawSetTransform(Vector2.Zero, 0.0f, Vector2.One);
    }

    public void DisplayPatch(Patch patch)
    {
        Patch = patch;
        PlacedPatch = null;
        QueueRedraw();
    }

    public void DisplayPlacedPatch(PlacedPatch placedPatch)
    {
        PlacedPatch = placedPatch;
        Patch = placedPatch.Patch;
        QueueRedraw();
    }

    public void Clear()
    {
        Patch = null;
        PlacedPatch = null;
        QueueRedraw();
    }

    private Texture2D GetCurrentTexture()
    {
        var patchId = PlacedPatch?.Patch.Id ?? Patch?.Id;

        if (patchId == null)
        {
            return null;
        }

        var directory = PlacedPatch?.IsMirrored == true ? MirroredTextureDirectory : OriginalTextureDirectory;
        var texturePath = $"{directory}/{patchId}.png";
        return ResourceLoader.Load<Texture2D>(texturePath);
    }

    private float GetCurrentRotationRadians()
    {
        var rotation = PlacedPatch?.Rotation ?? 0;
        // 把角度转成弧度
        return Mathf.DegToRad(rotation);
    }
}
