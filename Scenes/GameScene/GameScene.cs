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

		patchShop.PatchSelected += (patchOffset, sourceCenterGlobal, dragOffsetFromCenter) =>
		{
			patchShop.HidePatchAtOffset(patchOffset);
			patchBoard.BeginDragFromShop(patchOffset, sourceCenterGlobal, dragOffsetFromCenter);
		};

		patchBoard.DragCancelled += patchShop.RestoreHiddenPatch;
	}
}
