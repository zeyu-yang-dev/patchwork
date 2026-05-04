using System;
using Godot;
using Patchwork.Service;
using PatchBoardView = Patchwork.Scenes.PatchBoard.PatchBoardView;

namespace Patchwork.Scenes.ActivePatch;

// 是PatchBoardView下的节点
public partial class ActivePatchView : Control
{
	public event Action<Vector2> DragStarted;
	
	private const string OriginalTextureDirectory = "res://Assets/Patches/original";
	private const string MirroredTextureDirectory = "res://Assets/Patches/mirrored";
	private const int MatrixDimension = 5;
	private const float MatrixCellSize = 50.0f;
	private static readonly Vector2 ViewSize = new(MatrixDimension * MatrixCellSize, MatrixDimension * MatrixCellSize);
	public  static readonly Vector2 TopLeftToCenterOffset = ViewSize / 2.0f; // 从中心到左上角的距离
	private static readonly Color UnplaceablePatchModulate = new(1.0f, 1.0f, 1.0f, 0.55f);
	
	private TextureRect _textureDisplay;
	private RootService _rootService;
	
	// =================================================================================================================

	public override void _Ready()
	{
		Size = ViewSize;
		MouseFilter = MouseFilterEnum.Pass;

		_textureDisplay = GetNode<TextureRect>("TextureDisplay");
		_textureDisplay.Size = ViewSize;
		_textureDisplay.PivotOffset = TopLeftToCenterOffset;
	}
	
	// Initialize处理场景树外部的依赖注入
	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
		_rootService.StateChanged += OnGameStateChanged;
	}

	// =================================================================================================================

	// 触发从shop处或者从board里开始拖动ActivePatchView
	// _GuiInput 是 Godot 给 Control 节点提供的输入回调方法。
	// 当这个 Control 节点收到 GUI 输入事件时，Godot 会自动调用这个方法。
	public override void _GuiInput(InputEvent @event)
	{
		// 只在鼠标左键按下的那一瞬间会执行，持续按住不会反复触发
		if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true }) return;
		
		var centerToCursorOffset = GetLocalMousePosition() - TopLeftToCenterOffset;
		// 通知所有订阅了DragStarted事件的订阅者执行对应的函数，需要把那个函数的参数从这里传入
		DragStarted?.Invoke(centerToCursorOffset);
		
		// 告诉 Godot：这个输入事件已经被当前控件处理掉了，不要再继续传下去
		AcceptEvent();
	}
	
	// =================================================================================================================
	
	// 拿到一个原始或者镜像过的patch的texture
	private Texture2D GetCurrentTexture()
	{
		var currentPlacedPatch = _rootService.CurrentGame.CurrentPlacedPatch
								 ?? throw new InvalidOperationException("CurrentPlacedPatch is null.");
		
		var patchId = currentPlacedPatch.Patch.Id;
		var directory = currentPlacedPatch.IsMirrored ? MirroredTextureDirectory : OriginalTextureDirectory;
		
		return ResourceLoader.Load<Texture2D>($"{directory}/{patchId}.png");
	}

	// 将rotation的角度转换为弧度
	private float GetCurrentRotationRadians()
	{
		var currentPlacedPatch = _rootService.CurrentGame.CurrentPlacedPatch
								 ?? throw new InvalidOperationException("CurrentPlacedPatch is null.");
		
		var rotation = currentPlacedPatch.Rotation;
		return Mathf.DegToRad(rotation);
	}
	
	// 由Service层驱动的外观刷新函数
	private void RefreshVisual()
	{
		if (_rootService.CurrentGame.CurrentPlacedPatch == null)
		{
			Visible = false;
			_textureDisplay.Visible = false;
		}
		else
		{
			Visible = true;
			_textureDisplay.Visible = true;
			_textureDisplay.Texture = GetCurrentTexture();
			_textureDisplay.Rotation = GetCurrentRotationRadians();
			
			// 如果还没有坐标，不能根据IsPlaceable()决定不透明度，因为IsPlaceable()依赖坐标，
			// 然而，如果刚从商店里拖出来，是没有坐标的，但是这个时候也需要更新不透明度，
			// 如果有CurrentPlacedPatch但是它没坐标，说明刚从商店拖出来，这个时候要不透明显示。
			var coordinate = _rootService.CurrentGame.CurrentPlacedPatch.Coordinate;
			if (coordinate == null)
			{
				_textureDisplay.Modulate = Colors.White;
			}
			else
			{
				// 将_activePatchView按照格子吸附，也就是更新位置
				Position = GetLocalPositionFromCoordinate(coordinate.Value.col, coordinate.Value.row);
				
				// 在有坐标的情况下，按照是否能放置更新不透明度
				_textureDisplay.Modulate = _rootService.PlayerActionService.IsPlaceable() 
					? Colors.White 
					: UnplaceablePatchModulate;
			}
		}
	}
	
	// 由中心点的坐标得到它在patch board中的相对位置
	private static Vector2 GetLocalPositionFromCoordinate(int col, int row)
	{
		const float cellSize = PatchBoardView.BoardCellSize;
		// 先计算出cellCenter，也就是_activePatchView中心点所在的格子的中心点坐标
		var cellCenter = new Vector2((col + 0.5f) * cellSize, (row + 0.5f) * cellSize);
		// patch的应该移动到的位置：
		return cellCenter - TopLeftToCenterOffset;
	}
	
	// 由Service层驱动的刷新函数
	private void OnGameStateChanged()
	{
		RefreshVisual();
	}
}
