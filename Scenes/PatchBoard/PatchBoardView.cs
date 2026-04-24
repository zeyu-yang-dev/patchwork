using System;
using Godot;
using Patchwork.Service;
using ActivePatchView = Patchwork.Scenes.ActivePatch.ActivePatchView;

namespace Patchwork.Scenes.PatchBoard;

public partial class PatchBoardView : Panel
{
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
		_rootService.StateChanged += OnGameStateChanged;
		_activePatchView.Initialize(rootService);
	}
	
	// =================================================================================================================

	// 使得_activePatchView在拖动时随鼠标移动
	// `_Process()` 是逐帧回调
	// 只要节点处于可处理状态，Godot 就会在运行中每一帧调用它一次
	public override void _Process(double delta)
	{
		// 只处理正在拖拽的情况
		if (_interactionState is not (InteractionState.Dragging)) return;
		
		// 根据鼠标的位置计算出_activePatchView的中心的目标位置
		var activePatchViewCenterGlobal = GetGlobalMousePosition() - _centerToCursorOffset;
		_activePatchView.Position = GetLocalPositionFromGlobalCenter(activePatchViewCenterGlobal);
	}
	
	// 目前这个函数只处理鼠标松开事件
	public override void _Input(InputEvent @event)
	{
		// 只在拖动过程中触发，这个过滤很必要，否则在任何阶段鼠标松开都会触发
		if (_interactionState is not (InteractionState.Dragging)) return;
		
		// 只响应鼠标左键松开
		if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false }) return;
		
		// 如果_activePatchView的中心的全局坐标落在board的矩形区域内
		if (GetGlobalRect().HasPoint(ActivePatchViewCenterGlobal))
		{
			CapturePatch();
		}
		else
		{
			_rootService.PatchService.PutBackPatch();
		}
		
		_interactionState = InteractionState.Idle;
	}
	
	// =================================================================================================================
	
	// 对PatchShop的事件PatchSelected的响应，注意没有直接订阅，而是通过GameScene协调
	public void OnDragStartedFromShop(int patchOffset, Vector2 buttonCenterGlobal, Vector2 centerToCursorOffset)
	{
		_rootService.PatchService.TakePatch(patchOffset); // 只影响domain层的CurrentPlacedPatch
		
		// 用_activePatchView显示商店中被选中的patch
		// _activePatchView.Position指的是_activePatchView在PatchBoard中的相对坐标
		_activePatchView.Position = GetLocalPositionFromGlobalCenter(buttonCenterGlobal);
		
		_centerToCursorOffset = centerToCursorOffset;
		_interactionState = InteractionState.Dragging;
	}
	
	// 订阅了ActivePatchView的DragStarted事件
	// 更新_centerToCursorOffset并改变状态，使得_Process()可以让_activePatchView被拖动
	private void OnDragStartedFromBoard(Vector2 centerToCursorOffset)
	{
		_centerToCursorOffset = centerToCursorOffset;
		_interactionState = InteractionState.Dragging;
	}
	
	// =================================================================================================================
	
	private void CapturePatch()
	{
		// 向下取整
		var col = Mathf.FloorToInt(ActivePatchViewCenterLocal.X / BoardCellSize);
		var row = Mathf.FloorToInt(ActivePatchViewCenterLocal.Y / BoardCellSize);
		
		_rootService.PatchService.MovePatch(col, row);
		
		// 将_activePatchView按照格子吸附
		_activePatchView.Position = GetLocalPositionFromCoordinate(col, row);
		// 根据是否能放置改变_activePatchView的不透明度
		_activePatchView.Modulate = _rootService.PlayerActionService.IsPlaceable()
			? Colors.White
			: new Color(1.0f, 1.0f, 1.0f, 0.35f);
	}
	
	// 当一个patch在shop中被选中时，从事件PatchSelected得到的是它的中心的绝对坐标，
	// 需要将这个绝对坐标转换为PatchBoard中的相对坐标
	private Vector2 GetLocalPositionFromGlobalCenter(Vector2 activePatchViewCenterGlobal)
	{
		// _activePatchView的左上角的绝对坐标
		var topLeftGlobal = activePatchViewCenterGlobal - ActivePatchView.TopLeftToCenterOffset;
		// 将patchTopLeftGlobal从绝对坐标转换为在PatchBoard的坐标系中的相对坐标
		return GetGlobalTransform().AffineInverse() * topLeftGlobal;
	}
	
	// 由_activePatchView中心点的坐标得到它的位置
	private static Vector2 GetLocalPositionFromCoordinate(int col, int row)
	{
		// 先计算出cellCenter，也就是_activePatchView中心点所在的格子的中心点坐标
		var cellCenter = new Vector2((col + 0.5f) * BoardCellSize, (row + 0.5f) * BoardCellSize);
		// patch的目标位置
		return cellCenter - ActivePatchView.TopLeftToCenterOffset;
	}
	
	// 由Service层驱动的刷新函数
	private void OnGameStateChanged()
	{
		
	}
}
