using System;
using Godot;
using Patchwork.Service;
using ActivePatchView = Patchwork.Scenes.ActivePatch.ActivePatchView;

namespace Patchwork.Scenes.PatchBoard;

public partial class PatchBoardView : Panel
{
	private const int BoardSize = 9;
	private const float BoardCellSize = 50.0f;
	
	// 决定_Process和_UnhandledInput如何处理鼠标
	private enum InteractionState
	{
		Idle, // 没有拖拽
		Dragging // 正在拖拽
	}
	private InteractionState _interactionState = InteractionState.Idle;
	private RootService _rootService;
	private ActivePatchView _activePatchView;
	
	// _activePatchView在PatchBoard内部的本地坐标
	private Vector2 ActivePatchViewCenterLocal => _activePatchView.Position + ActivePatchView.TopLeftToCenterOffset;
	// _activePatchView在整个画面中的全局坐标
	private Vector2 ActivePatchViewCenterGlobal => GetGlobalTransform() * ActivePatchViewCenterLocal;
	// 从_activePatchView中心指向光标位置的向量
	// 需要在从shop开始拖动，或者从board开始拖动后更新
	// 更新的地方在两个事件处理方法（event handler）中
	private Vector2 _centerToCursorOffset;
	
	// =================================================================================================================

	// _Ready处理场景树内部的事情：1. 子节点获取 2. 节点属性初始化 3. 场景内节点之间的事件连接
	public override void _Ready()
	{
		_activePatchView = GetNode<ActivePatchView>("ActivePatchView");
		
		_activePatchView.DragStarted += OnDragStartedFromBoard;
	}
	
	// Initialize处理场景树外部的依赖注入
	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
		_rootService.StateChanged += RefreshVisual;
		
		_activePatchView.Initialize(rootService);
	}
	
	// =================================================================================================================

	// `_Process()` 是逐帧回调
	// 只要节点处于可处理状态，Godot 就会在运行中每一帧调用它一次
	public override void _Process(double delta)
	{
		// 只处理 开始从shop或者board拖动后 
		if (_interactionState is not (InteractionState.Dragging))
		{
			return;
		}

		
		
		var patchCenterGlobal = GetGlobalMousePosition() - _centerToCursorOffset;
		UpdateActivePatchPosition(patchCenterGlobal);
	}

	// 当有输入事件产生，并且这个事件还没有被前面的输入处理链消费掉时，Godot 会把它传进来
	// 目前这个函数只处理鼠标松开事件
	public override void _Input(InputEvent @event)
	{
		// 只处理 开始从shop或者board拖动后 
		if (_interactionState is not (InteractionState.Dragging))
		{
			return;
		}
		
		
		// 只响应鼠标松开
		if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false })
		{
			return;
		}
		
			
		if (ContainsGlobalPoint(ActivePatchViewCenterGlobal))
		{
			UpdatePlacement();
			_interactionState = InteractionState.Idle;
		}
		else
		{
			// 从视觉上将patch放回shop
			_rootService.PatchService.PutBackPatch();
			_interactionState = InteractionState.Idle;
			
			GD.Print("return patch to shop");	
		}
			
	}
	
	// =================================================================================================================

	

	// 对PatchShop的事件PatchSelected的响应，注意没有直接订阅，而是通过GameScene协调
	public void OnDragStartedFromShop(int patchOffset, Vector2 buttonCenterGlobal, Vector2 centerToCursorOffset)
	{
		
		_rootService.PatchService.TakePatch(patchOffset); // 只影响domain层的CurrentPlacedPatch
		_centerToCursorOffset = centerToCursorOffset;
		
		_interactionState = InteractionState.Dragging;
		
		// 用_activePatchView显示商店中被选中的patch
		// _activePatchView.DisplayPlacedPatch(_rootService.CurrentGame.CurrentPlacedPatch);
		// _activePatchView.Position指的是_activePatchView在PatchBoard中的相对坐标
		_activePatchView.Position = GetLocalPositionFromGlobalCenter(buttonCenterGlobal);
		// 给节点整体乘一个颜色，乘以白色表示不变
		_activePatchView.Modulate = Colors.White;
		_activePatchView.Visible = true;
	}
	
	// 订阅了ActivePatchView的DragStarted事件
	// 更新_centerToCursorOffset并改变状态，使得_Process()可以让_activePatchView被拖动
	private void OnDragStartedFromBoard(ActivePatchView _, Vector2 centerToCursorOffset)
	{
		// 只响应从board开始的拖动
		if (_interactionState != InteractionState.Idle)
		{
			return;
		}

		_centerToCursorOffset = centerToCursorOffset;
		_interactionState = InteractionState.Dragging;
	}

	

	

	// 从视觉上将patch放回shop(Domain层的shop中的patch其实一直没有减少)
	// 在 从shop拖出patch但没有拖入 或者 将已经放置的patch拖出board 时调用
	// public void CancelDragFromShop()
	// {
	// 	// 只影响Domain层的CurrentPlacedPatch
	// 	_rootService.PatchService.PutBackPatch();
	// }
	
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
		return cellCenter - ActivePatchView.TopLeftToCenterOffset;
	}

	// 当一个patch在shop中被选中时，从事件PatchSelected得到的是它的中心的绝对坐标，
	// 需要将这个绝对坐标转换为PatchBoard中的相对坐标
	private Vector2 GetLocalPositionFromGlobalCenter(Vector2 buttonCenterGlobal)
	{
		// _activePatchView的左上角的绝对坐标
		var patchTopLeftGlobal = buttonCenterGlobal - ActivePatchView.TopLeftToCenterOffset;
		// 将patchTopLeftGlobal从绝对坐标转换为在PatchBoard的坐标系中的相对坐标
		return GetGlobalTransform().AffineInverse() * patchTopLeftGlobal;
	}
	
	// 由Service层驱动的外观刷新函数
	// TODO:检查是否已经改为按GameState刷新
	private void RefreshVisual()
	{
		
	}

	
}
