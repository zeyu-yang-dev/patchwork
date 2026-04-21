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
    public static readonly Vector2 CenterOffset = ViewSize / 2.0f;

    private const string OriginalTextureDirectory = "res://Assets/Patches/original";
    private const string MirroredTextureDirectory = "res://Assets/Patches/mirrored";

    public PlacedPatch PlacedPatch { get; private set; }

    public (int col, int row)? Coordinate => PlacedPatch?.Coordinate;

    public override void _Ready()
    {
        Size = ViewSize;
        PivotOffset = CenterOffset;
        MouseFilter = MouseFilterEnum.Pass;
    }

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
        DragStarted?.Invoke(this, dragOffsetFromCenter);
        AcceptEvent();
    }

    public override void _Draw()
    {
        var texture = GetCurrentTexture();

        if (texture == null)
        {
            return;
        }

        DrawSetTransform(CenterOffset, GetCurrentRotationRadians(), Vector2.One);
        DrawTextureRect(texture, new Rect2(-CenterOffset, ViewSize), false);
        DrawSetTransform(Vector2.Zero, 0.0f, Vector2.One);
    }

    public void DisplayPlacedPatch(PlacedPatch placedPatch)
    {
        PlacedPatch = placedPatch;
        QueueRedraw();
    }

    public void Clear()
    {
        PlacedPatch = null;
        QueueRedraw();
    }

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

    private float GetCurrentRotationRadians()
    {
        var rotation = PlacedPatch?.Rotation ?? 0;
        return Mathf.DegToRad(rotation);
    }
}
