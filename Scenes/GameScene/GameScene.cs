using Godot;
using Patchwork.Service;

namespace Patchwork.Scenes.GameScene;

public partial class GameScene : Control
{
	private Patchwork.Scenes.PatchBoard.PatchBoardView _patchBoardView;
	private Patchwork.Scenes.PatchShop.PatchShopView _patchShopView;

	public RootService RootService { get; private set; }

	// =================================================================================================================
	
	// _Ready处理场景树内部的事情：1. 子节点获取 2. 节点属性初始化 3. 场景内节点之间的事件连接
	public override void _Ready()
	{
		_patchBoardView = GetNode<Patchwork.Scenes.PatchBoard.PatchBoardView>("PatchBoardView");
		_patchShopView = GetNode<Patchwork.Scenes.PatchShop.PatchShopView>("PatchShopView");
		
		// 1. 按钮的index 2. 按钮的中心点的全局坐标 3. 从按钮中心指向鼠标位置的矢量
		_patchShopView.PatchSelected += (patchOffset, buttonCenterGlobal, centerToCursorOffset) =>
		{
			
			_patchBoardView.StartDragFromShop(patchOffset, buttonCenterGlobal, centerToCursorOffset);
		};

		
	}

	// Initialize处理场景树外部的依赖注入
	public void Initialize(RootService rootService)
	{
		RootService = rootService;

		_patchBoardView.Initialize(rootService);
		_patchShopView.Initialize(rootService);
	}
	
	// =================================================================================================================
}
