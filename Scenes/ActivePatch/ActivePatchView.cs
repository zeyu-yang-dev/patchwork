using System;
using Godot;
using Patchwork.Domain;

namespace Patchwork.Scenes.ActivePatch;

// 是PatchBoardView下的节点
public partial class ActivePatchView : Control
{
	public event Action<ActivePatchView, Vector2> DragStarted;

	public const int MatrixSize = 5;
	public const float CellSize = 50.0f;
	public static readonly Vector2 ViewSize = new(MatrixSize * CellSize, MatrixSize * CellSize);
	public static readonly Vector2 TopLeftToCenterOffset = ViewSize / 2.0f; // 从中心到左上角的距离

	private const string OriginalTextureDirectory = "res://Assets/Patches/original";
	private const string MirroredTextureDirectory = "res://Assets/Patches/mirrored";

	private TextureRect _textureDisplay;

	public PlacedPatch PlacedPatch { get; private set; }

	// =================================================================================================================

	public override void _Ready()
	{
		Size = ViewSize;
		MouseFilter = MouseFilterEnum.Pass;

		_textureDisplay = GetNode<TextureRect>("TextureDisplay");
		_textureDisplay.Size = ViewSize;
		_textureDisplay.PivotOffset = TopLeftToCenterOffset;
		RefreshVisual();
	}

	// =================================================================================================================

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

		var dragOffsetFromCenter = GetLocalMousePosition() - TopLeftToCenterOffset;
		// 通知所有订阅了DragStarted事件的订阅者执行对应的函数，需要把那个函数的参数从这里传入
		DragStarted?.Invoke(this, dragOffsetFromCenter);
		AcceptEvent();
	}

	// =================================================================================================================

	public void DisplayPlacedPatch(PlacedPatch placedPatch)
	{
		PlacedPatch = placedPatch;
		// 把显示PlacedPatch的工作交给TextureRect子节点
		RefreshVisual();
	}

	public void Clear()
	{
		PlacedPatch = null;
		RefreshVisual();
	}

	// =================================================================================================================

	private void RefreshVisual()
	{
		var texture = GetCurrentTexture();
		_textureDisplay.Texture = texture;
		_textureDisplay.Rotation = GetCurrentRotationRadians();
		_textureDisplay.Visible = texture != null;
	}

	// 拿到一个原始或者镜像过的patch的texture
	private Texture2D GetCurrentTexture()
	{
		var patchId = PlacedPatch?.Patch.Id;
		if (patchId == null)
		{
			return null;
		}

		var directory = PlacedPatch.IsMirrored ? MirroredTextureDirectory : OriginalTextureDirectory;
		return ResourceLoader.Load<Texture2D>($"{directory}/{patchId}.png");
	}

	// 将rotation的角度转换为弧度
	private float GetCurrentRotationRadians()
	{
		var rotation = PlacedPatch?.Rotation ?? 0;
		return Mathf.DegToRad(rotation);
	}
}
