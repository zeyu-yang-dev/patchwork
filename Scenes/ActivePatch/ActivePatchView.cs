using System;
using Godot;
using Patchwork.Service;


namespace Patchwork.Scenes.ActivePatch;

// 是PatchBoardView下的节点
public partial class ActivePatchView : Control
{
	private const int MatrixSize = 5;
	private const float CellSize = 50.0f;
	private static readonly Vector2 ViewSize = new(MatrixSize * CellSize, MatrixSize * CellSize);
	public static readonly Vector2 TopLeftToCenterOffset = ViewSize / 2.0f; // 从中心到左上角的距离
	private const string OriginalTextureDirectory = "res://Assets/Patches/original";
	private const string MirroredTextureDirectory = "res://Assets/Patches/mirrored";
	
	public event Action<ActivePatchView, Vector2> DragStarted;
	
	private RootService _rootService;
	private TextureRect _textureDisplay;

	
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
	
	// Initialize处理场景树外部的依赖注入
	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
		_rootService.StateChanged += RefreshVisual;
	}

	// =================================================================================================================

	// 商店中的点击入口：
	// _GuiInput 是 Godot 给 Control 节点提供的输入回调方法。
	// 当这个 Control 节点收到 GUI 输入事件时，Godot 会自动调用这个方法。
	public override void _GuiInput(InputEvent @event)
	{
		// 只响应鼠标左键按下
		if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
		{
			return;
		}

		// 如果CurrentPlacedPatch == null，那么当然没必要开始拖动
		if (_rootService.CurrentGame.CurrentPlacedPatch == null)
		{
			return;
		}

		var dragOffsetFromCenter = GetLocalMousePosition() - TopLeftToCenterOffset;
		// 通知所有订阅了DragStarted事件的订阅者执行对应的函数，需要把那个函数的参数从这里传入
		DragStarted?.Invoke(this, dragOffsetFromCenter);
		AcceptEvent();
	}

	// =================================================================================================================

	

	// =================================================================================================================

	

	// 拿到一个原始或者镜像过的patch的texture
	private Texture2D GetCurrentTexture()
	{
		var placedPatch = _rootService.CurrentGame.CurrentPlacedPatch;
		
		var patchId = placedPatch?.Patch.Id;
		if (patchId == null)
		{
			return null;
		}

		var directory = placedPatch.IsMirrored ? MirroredTextureDirectory : OriginalTextureDirectory;
		return ResourceLoader.Load<Texture2D>($"{directory}/{patchId}.png");
	}

	// 将rotation的角度转换为弧度
	private float GetCurrentRotationRadians()
	{
		var placedPatch = _rootService.CurrentGame.CurrentPlacedPatch;
		
		var rotation = placedPatch?.Rotation ?? 0;
		return Mathf.DegToRad(rotation);
	}
	
	// 由Service层驱动的外观刷新函数
	private void RefreshVisual()
	{
		var texture = GetCurrentTexture();
		_textureDisplay.Texture = texture;
		_textureDisplay.Rotation = GetCurrentRotationRadians();
		_textureDisplay.Visible = texture != null;
	}
}
