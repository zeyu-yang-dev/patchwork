using Godot;
using Patchwork.Service;

namespace Patchwork.Scenes.GameScene;

public partial class GameScene : Control
{
	public RootService RootService { get; private set; }

	public void Initialize(RootService rootService)
	{
		RootService = rootService;

		var patchBoard = GetNode<Patchwork.Scenes.PatchBoard.PatchBoard>("PatchBoard");
		var patchShop = GetNode<Patchwork.Scenes.PatchShop.PatchShop>("PatchShop");

		patchBoard.Initialize(rootService);
		patchShop.Initialize(rootService);

		// 1. 按钮的index 2. 按钮的中心点的全局坐标 3. 从按钮中心指向鼠标位置的矢量
		patchShop.PatchSelected += (patchOffset, buttonCenterGlobal, centerToCursorOffset) =>
		{
			patchShop.HideButtonAtIndex(patchOffset);
			patchBoard.StartDragFromShop(patchOffset, buttonCenterGlobal, centerToCursorOffset);
		};

		patchBoard.DragCancelled += patchShop.RestoreHiddenButton;
	}
}
