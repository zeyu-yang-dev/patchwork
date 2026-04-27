using Godot;
using Patchwork.Service;

namespace Patchwork.Scenes.GameScene;

public partial class GameScene : Control
{
	private PatchBoard.PatchBoardView _patchBoardView;
	private PatchShopView _patchShopView;
	private ControlPanelView _controlPanelView;
	private PatchBoardDisplay _patchBoardDisplayLeft;
	private PatchBoardDisplay _patchBoardDisplayRight;
	private TimelineDisplay _timelineDisplay;

	public RootService RootService { get; private set; }

	// =================================================================================================================
	
	// _Ready处理场景树内部的事情：1. 子节点获取 2. 节点属性初始化 3. 场景内节点之间的事件连接
	public override void _Ready()
	{
		_patchBoardView = GetNode<PatchBoard.PatchBoardView>("PatchBoardView");
		_patchShopView = GetNode<PatchShopView>("PatchShopView");
		_controlPanelView = GetNode<ControlPanelView>("ControlPanelView");
		_patchBoardDisplayLeft = GetNode<PatchBoardDisplay>("PatchBoardDisplayLeft");
		_patchBoardDisplayRight = GetNode<PatchBoardDisplay>("PatchBoardDisplayRight");
		_patchBoardDisplayLeft.Target = DisplayTarget.CurrentPlayer;
		_patchBoardDisplayRight.Target = DisplayTarget.OtherPlayer;
		_timelineDisplay =  GetNode<TimelineDisplay>("TimelineDisplay");
		
		// 让_patchBoardView订阅_patchShopView的事件，不能直接订阅是因为它们不相互持有
		// 1. 按钮的index 2. 按钮的中心点的全局坐标 3. 从按钮中心指向鼠标位置的矢量
		_patchShopView.PatchSelected += _patchBoardView.OnDragStartedFromShop;
		
	}

	// Initialize处理场景树外部的依赖注入
	public void Initialize(RootService rootService)
	{
		RootService = rootService;

		_patchBoardView.Initialize(rootService);
		_patchShopView.Initialize(rootService);
		_controlPanelView.Initialize(rootService);
		_patchBoardDisplayLeft.Initialize(rootService);
		_patchBoardDisplayRight.Initialize(rootService);
		_timelineDisplay.Initialize(rootService);
	}
	
	// =================================================================================================================
}
