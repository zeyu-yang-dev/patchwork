using System;
using Godot;
using Patchwork.Service;

namespace Patchwork.Scenes.PatchBoard;

public partial class PatchBoard : Panel
{
	// 被GameScene订阅，GameScene会通知PatchShop把按钮重新显示
	public event Action DragCancelled;

	// 决定_Process和_UnhandledInput如何处理鼠标
	private enum InteractionState
	{
		Idle, // 没有拖拽
		DraggingFromShop, //从shop到board的路上
		HasPlacedPatch, // _activePatchView已经被board捕获
		DraggingFromBoard // 从board上开始再次拖动
	}
	private InteractionState _interactionState = InteractionState.Idle;

	private const int BoardSize = 9;
	private const float BoardCellSize = 50.0f;
	private const float BoardPixelSize = BoardSize * BoardCellSize;
	
	private RootService _rootService;
	
	private Patchwork.Scenes.ActivePatchView.ActivePatchView _activePatchView;
	// 在PatchBoard内部的坐标
	private Vector2 ActivePatchViewCenterLocal => _activePatchView.Position + ActivePatchView.ActivePatchView.TopLeftToCenterOffset;
	private Vector2 ActivePatchViewCenterGlobal => GetGlobalTransform() * ActivePatchViewCenterLocal;
	
	// 从_activePatchView中心指向光标位置的向量
	// 需要在从shop开始拖动，或者从board开始拖动后更新
	private Vector2 _centerToCursorOffset;
	
	// =================================================================================================================

	public override void _Ready()
	{
		// 动态新建一个子节点
		_activePatchView = new Patchwork.Scenes.ActivePatchView.ActivePatchView
		{
			Name = "ActivePatchView",
			Visible = false
		};
		
		_activePatchView.DragStarted += OnActivePatchViewDragStarted;
		AddChild(_activePatchView);
	}

	// `_Process()` 是逐帧回调
	// 只要节点处于可处理状态，Godot 就会在运行中每一帧调用它一次
	public override void _Process(double delta)
	{
		// 只处理 开始从shop或者board拖动后 
		if (_interactionState is not (InteractionState.DraggingFromShop or InteractionState.DraggingFromBoard))
		{
			return;
		}

		// var patchCenterGlobal = GetGlobalMousePosition() - _centerToCursorOffset;
		//
		// if (_interactionState == InteractionState.DraggingFromShop)
		// {
		// 	UpdateActivePatchPosition(patchCenterGlobal);
		//
		// 	if (ContainsGlobalPoint(patchCenterGlobal))
		// 	{
		// 		_interactionState = InteractionState.HasPlacedPatch;
		// 		UpdatePlacement(patchCenterGlobal);
		// 	}
		//
		// 	return;
		// }
		//
		// UpdatePlacement(patchCenterGlobal);
		
		var patchCenterGlobal = GetGlobalMousePosition() - _centerToCursorOffset;
		UpdateActivePatchPosition(patchCenterGlobal);
	}

	// 当有输入事件产生，并且这个事件还没有被前面的输入处理链消费掉时，Godot 会把它传进来
	// 目前这个函数只处理鼠标松开事件
	public override void _Input(InputEvent @event)
	{
		// 只处理 开始从shop或者board拖动后 
		if (_interactionState is not (InteractionState.DraggingFromShop or InteractionState.DraggingFromBoard))
		{
			return;
		}
		
		
		// 只响应鼠标松开
		if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false })
		{
			return;
		}

		// switch (_interactionState)
		// {
		// 	case InteractionState.DraggingFromShop:
		// 		CancelDragFromShop();
		// 		_interactionState = InteractionState.Idle;
		// 		break;
		// 	
		// 	case InteractionState.DraggingFromBoard:
		// 		_interactionState = InteractionState.HasPlacedPatch;
		// 		break;
		// }

		
		
			
		if (ContainsGlobalPoint(ActivePatchViewCenterGlobal))
		{
				
			UpdatePlacement();
			_interactionState = InteractionState.HasPlacedPatch;
				
		}
		else
		{
			// 从视觉上将patch放回shop
			CancelDragFromShop();
			_interactionState = InteractionState.Idle;
				
		}
			
	}
	
	// =================================================================================================================

	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
	}

	// 对PatchShop的事件PatchSelected的响应，注意没有直接订阅，而是通过GameScene协调
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
	
	// 订阅了ActivePatchView的DragStarted事件
	// 更新_centerToCursorOffset并改变状态，使得_Process()可以让_activePatchView被拖动
	private void OnActivePatchViewDragStarted(
		Patchwork.Scenes.ActivePatchView.ActivePatchView _, 
		Vector2 centerToCursorOffset)
	{
		// 只响应从board开始的拖动
		if (_interactionState != InteractionState.HasPlacedPatch)
		{
			return;
		}

		_centerToCursorOffset = centerToCursorOffset;
		_interactionState = InteractionState.DraggingFromBoard;
	}

	

	

	// 从视觉上将patch放回shop(Domain层的shop中的patch其实一直没有减少)
	// 在 从shop拖出patch但没有拖入 或者 将已经放置的patch拖出board 时调用
	public void CancelDragFromShop()
	{
		// 只影响Domain层的CurrentPlacedPatch
		_rootService.PatchService.PutBackPatch();
		
		// 清空并隐藏_activePatchView
		_activePatchView.Clear();
		_activePatchView.Visible = false;
		_activePatchView.Modulate = Colors.White;
		
		// 通过GameScene调用PatchShop的RestoreHiddenButton
		DragCancelled?.Invoke();
	}
	
	// =================================================================================================================
	
	private void UpdatePlacement()
	{
		
		// 向下取整
		var col = Mathf.FloorToInt(ActivePatchViewCenterLocal.X / BoardCellSize);
		var row = Mathf.FloorToInt(ActivePatchViewCenterLocal.Y / BoardCellSize);

		// 也就是说，没拖动进board或者拖出了board
		if (!IsInsideBoard(col, row))
		{
			return;
		}

		_rootService.PatchService.MovePatch(col, row);
		
		// 将_activePatchView按照格子吸附
		_activePatchView.Position = GetDestinyCoordinateForPatch(col, row);
		_activePatchView.Modulate = IsCurrentPatchPlaceable(col, row)
			? Colors.White
			: new Color(1.0f, 1.0f, 1.0f, 0.35f);
		_activePatchView.Visible = true;
	}
	
	// 判断一个“全局坐标点”是否落在当前 PatchBoard 这个控件的屏幕范围内
	private bool ContainsGlobalPoint(Vector2 globalPoint)
	{
		// GetGlobalRect()是 Control 的方法，返回当前 PatchBoard 在全局坐标系中的矩形区域
		return GetGlobalRect().HasPoint(globalPoint);
	}

	// 使_activePatchView在被拖动时移动
	private void UpdateActivePatchPosition(Vector2 patchCenterGlobal)
	{
		// 将_activePatchView的中心绝对坐标转换为它在PatchBoard中的相对坐标
		_activePatchView.Position = GetLocalPositionFromGlobalCenter(patchCenterGlobal);
		// 给节点整体乘一个颜色，乘以白色表示不变
		_activePatchView.Modulate = Colors.White;
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

	private static Vector2 GetDestinyCoordinateForPatch(int col, int row)
	{
		// (col, row)格子的中心点坐标
		var cellCenter = new Vector2((col + 0.5f) * BoardCellSize, (row + 0.5f) * BoardCellSize);
		// patch目标位置
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

	
}
