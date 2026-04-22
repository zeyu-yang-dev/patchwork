using System;
using Godot;
using Patchwork.Service;

namespace Patchwork.Scenes.PatchBoard;

public partial class PatchBoard : Panel
{
	public event Action DragCancelled;

	// 决定_Process和_UnhandledInput如何处理鼠标
	private enum InteractionState
	{
		Idle, // 没有拖拽
		DraggingFromShop, //从shop到board的路上
		PlacingPatch, // _activePatchView已经被board捕获
		DraggingPlacedPatch // 在board上开始再次拖动
	}

	private const int BoardSize = 9;
	private const float BoardCellSize = 50.0f;
	private const float BoardPixelSize = BoardSize * BoardCellSize;
	 

	private RootService _rootService;
	private Control _placedPatchLayer;
	private Patchwork.Scenes.ActivePatchView.ActivePatchView _activePatchView;
	private InteractionState _interactionState = InteractionState.Idle;
	// 从_activePatchView中心指向光标位置的向量
	private Vector2 _centerToCursorOffset;
	
	// =================================================================================================================

	public override void _Ready()
	{
		_placedPatchLayer = new Control
		{
			Name = "PlacedPatchLayer",
			MouseFilter = MouseFilterEnum.Ignore
		};
		_placedPatchLayer.SetAnchorsPreset(LayoutPreset.FullRect);
		AddChild(_placedPatchLayer);

		_activePatchView = new Patchwork.Scenes.ActivePatchView.ActivePatchView
		{
			Name = "ActivePatchView",
			Visible = false
		};
		_activePatchView.DragStarted += OnActivePatchViewDragStarted;
		AddChild(_activePatchView);
	}

	// 在拖拽过程中更新
	public override void _Process(double delta)
	{
		if (_interactionState is not (InteractionState.DraggingFromShop or InteractionState.DraggingPlacedPatch))
		{
			return;
		}

		var patchCenterGlobal = GetGlobalMousePosition() - _centerToCursorOffset;

		if (_interactionState == InteractionState.DraggingFromShop)
		{
			UpdateFreeDragPosition(patchCenterGlobal);

			if (ContainsGlobalPoint(patchCenterGlobal))
			{
				_interactionState = InteractionState.PlacingPatch;
				UpdatePlacementFromPatchCenter(patchCenterGlobal);
			}

			return;
		}

		UpdatePlacementFromPatchCenter(patchCenterGlobal);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false })
		{
			return;
		}

		switch (_interactionState)
		{
			case InteractionState.DraggingFromShop:
				_rootService.PatchService.PutBackPatch();
				CancelDrag();
				DragCancelled?.Invoke();
				_interactionState = InteractionState.Idle;
				break;
			case InteractionState.DraggingPlacedPatch:
				_interactionState = InteractionState.PlacingPatch;
				break;
		}
	}
	
	// =================================================================================================================

	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
		Refresh();
	}

	// 对PatchShop的事件PatchSelected的响应
	public void StartDragFromShop(int patchOffset, Vector2 buttonCenterGlobal, Vector2 centerToCursorOffset)
	{
		if (_rootService?.CurrentGame?.CurrentPlacedPatch != null)
		{
			return;
		}

		_rootService.PatchService.TakePatch(patchOffset); // 只影响domain层的CurrentPlacedPatch
		_centerToCursorOffset = centerToCursorOffset;
		
		_interactionState = InteractionState.DraggingFromShop;
		
		// 用_activePatchView显示商店中被选中的patch
		_activePatchView.DisplayPlacedPatch(_rootService.CurrentGame.CurrentPlacedPatch);
		// _activePatchView.Position指的是_activePatchView在PatchBoard中的相对坐标
		_activePatchView.Position = GetLocalPositionFromGlobalCenter(buttonCenterGlobal);
		// 给节点整体乘一个颜色，乘以白色表示不变
		_activePatchView.Modulate = Colors.White;
		_activePatchView.Visible = true;
	}

	public void Refresh()
	{
		if (_rootService?.CurrentGame == null)
		{
			return;
		}

		foreach (var child in _placedPatchLayer.GetChildren())
		{
			child.QueueFree();
		}

		foreach (var placedPatch in _rootService.CurrentGame.CurrentPlayer.PatchBoard.PlacedPatches)
		{
			var activePatchView = new Patchwork.Scenes.ActivePatchView.ActivePatchView
			{
				MouseFilter = MouseFilterEnum.Ignore
			};

			_placedPatchLayer.AddChild(activePatchView);
			activePatchView.DisplayPlacedPatch(placedPatch);
			activePatchView.Position = GetPatchTopLeftFromCenter(placedPatch.Coordinate!.Value.col, placedPatch.Coordinate!.Value.row);
		}
	}

	// 判断一个“全局坐标点”是否落在当前 PatchBoard 这个控件的屏幕范围内
	public bool ContainsGlobalPoint(Vector2 globalPoint)
	{
		// GetGlobalRect()是 Control 的方法，返回当前 PatchBoard 在全局坐标系中的矩形区域
		return GetGlobalRect().HasPoint(globalPoint);
	}

	public void CancelDrag()
	{
		var currentPlacedPatch = _rootService?.CurrentGame?.CurrentPlacedPatch;

		if (currentPlacedPatch != null)
		{
			currentPlacedPatch.Coordinate = null;
		}

		_activePatchView.Clear();
		_activePatchView.Visible = false;
		_activePatchView.Modulate = Colors.White;
	}
	
	// =================================================================================================================

	private void UpdateFreeDragPosition(Vector2 patchCenterGlobal)
	{
		// 将_activePatchView的中心绝对坐标转换为它在PatchBoard中的相对坐标
		_activePatchView.Position = GetLocalPositionFromGlobalCenter(patchCenterGlobal);
		// 给节点整体乘一个颜色，乘以白色表示不变
		_activePatchView.Modulate = Colors.White;
		_activePatchView.Visible = true;
	}

	private void UpdatePlacementFromPatchCenter(Vector2 patchCenterGlobal)
	{
		var currentPlacedPatch = _rootService?.CurrentGame?.CurrentPlacedPatch;

		if (currentPlacedPatch == null)
		{
			CancelDrag();
			return;
		}

		var boardLocalCenter = GetGlobalTransform().AffineInverse() * patchCenterGlobal;
		var col = Mathf.FloorToInt(boardLocalCenter.X / BoardCellSize);
		var row = Mathf.FloorToInt(boardLocalCenter.Y / BoardCellSize);

		if (!IsInsideBoard(col, row))
		{
			return;
		}

		_rootService.PatchService.MovePatch(col, row);
		_activePatchView.Position = GetPatchTopLeftFromCenter(col, row);
		_activePatchView.Modulate = IsCurrentPatchPlaceable(col, row)
			? Colors.White
			: new Color(1.0f, 1.0f, 1.0f, 0.35f);
		_activePatchView.Visible = true;
	}

	private bool IsCurrentPatchPlaceable(int col, int row)
	{
		var currentGame = _rootService.CurrentGame;
		var currentPlacedPatch = currentGame.CurrentPlacedPatch;
		return currentGame.CurrentPlayer.PatchBoard.IsPlaceable(currentPlacedPatch, col, row);
	}

	private static bool IsInsideBoard(int col, int row)
	{
		return col >= 0 && col < BoardSize && row >= 0 && row < BoardSize;
	}

	private static Vector2 GetPatchTopLeftFromCenter(int col, int row)
	{
		var cellCenter = new Vector2((col + 0.5f) * BoardCellSize, (row + 0.5f) * BoardCellSize);
		return cellCenter - ActivePatchView.ActivePatchView.TopLeftToCenterOffset;
	}

	// 当一个patch在shop中被选中时，从事件PatchSelected得到的是它的中心的绝对坐标，
	// 需要将这个绝对坐标转换为PatchBoard中的相对坐标
	private Vector2 GetLocalPositionFromGlobalCenter(Vector2 buttonCenterGlobal)
	{
		// _activePatchView的左上角的绝对坐标
		var patchTopLeftGlobal = buttonCenterGlobal - ActivePatchView.ActivePatchView.TopLeftToCenterOffset;
		// 将patchTopLeftGlobal从绝对坐标转换为在PatchBoard的坐标系中的相对坐标
		return GetGlobalTransform().AffineInverse() * patchTopLeftGlobal;
	}

	private void OnActivePatchViewDragStarted(Patchwork.Scenes.ActivePatchView.ActivePatchView _, Vector2 dragOffsetFromCenter)
	{
		if (_interactionState != InteractionState.PlacingPatch)
		{
			return;
		}

		_centerToCursorOffset = dragOffsetFromCenter;
		_interactionState = InteractionState.DraggingPlacedPatch;
	}
}
