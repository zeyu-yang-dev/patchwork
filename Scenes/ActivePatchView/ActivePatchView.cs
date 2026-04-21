using System;
using Godot;
using Patchwork.Domain;

namespace Patchwork.Scenes.ActivePatchView;

public partial class ActivePatchView : Control
{
    public event Action<ActivePatchView, Vector2> DragStarted;

    public const int MatrixSize = 5;
    public const float CellSize = 50.0f;
    public static readonly Vector2 ViewSize = new(MatrixSize * CellSize, MatrixSize * CellSize);
    public static readonly Vector2 CenterOffset = ViewSize / 2.0f; // 从中心到左上角的距离

    private const string OriginalTextureDirectory = "res://Assets/Patches/original";
    private const string MirroredTextureDirectory = "res://Assets/Patches/mirrored";

    public PlacedPatch PlacedPatch { get; private set; }

    public (int col, int row)? Coordinate => PlacedPatch?.Coordinate;
    
    // =================================================================================================================

    public override void _Ready()
    {
        Size = ViewSize;
        PivotOffset = CenterOffset;
        MouseFilter = MouseFilterEnum.Pass;
    }

    // 商店中的点击入口：
    // _GuiInput 是 Godot 给 Control 节点提供的输入回调方法。
    // 当这个 Control 节点收到 GUI 输入事件时，Godot 会自动调用这个方法。
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
        {
            return;
        }

        if (PlacedPatch == null)
        {
            return;
        }

        var dragOffsetFromCenter = GetLocalMousePosition() - CenterOffset;
        // 通知所有订阅了DragStarted事件的订阅者执行对应的函数
        DragStarted?.Invoke(this, dragOffsetFromCenter);
        AcceptEvent();
    }

    // 当这个节点需要重绘时，Godot 会自动调用 _Draw()
    // 通过QueueRedraw()通知Godot调用_Draw()
    public override void _Draw()
    {
        var texture = GetCurrentTexture();

        if (texture == null)
        {
            return;
        }

        // 1. 把绘图原点移动到 CenterOffset 2. 按 GetCurrentRotationRadians() 给出的角度旋转 3. 缩放保持 1
        DrawSetTransform(CenterOffset, GetCurrentRotationRadians(), Vector2.One);
        // 实际绘制：1.按照texture 2. 从 (-125,-125) 开始画一个250x250的矩形  3. 不平铺贴图
        DrawTextureRect(texture, new Rect2(-CenterOffset, ViewSize), false);
        // 在把前面的绘图变换恢复成默认状态
        DrawSetTransform(Vector2.Zero, 0.0f, Vector2.One);
    }
    
    // =================================================================================================================

    public void DisplayPlacedPatch(PlacedPatch placedPatch)
    {
        PlacedPatch = placedPatch;
        // 把这个节点标记成“需要重画”，Godot 在合适的时机重新调用 _Draw()
        QueueRedraw();
    }

    public void Clear()
    {
        PlacedPatch = null;
        QueueRedraw();
    }
    
    // =================================================================================================================

    // 拿到一个原始或者镜像过的patch的texture
    private Texture2D GetCurrentTexture()
    {
        var patchId = PlacedPatch?.Patch.Id;

        if (patchId == null)
        {
            return null;
        }

        var directory = PlacedPatch.IsMirrored ? MirroredTextureDirectory : OriginalTextureDirectory;
        var texturePath = $"{directory}/{patchId}.png";
        return ResourceLoader.Load<Texture2D>(texturePath);
    }

    // 将rotation的角度转换为弧度
    private float GetCurrentRotationRadians()
    {
        var rotation = PlacedPatch?.Rotation ?? 0;
        return Mathf.DegToRad(rotation);
    }
}
