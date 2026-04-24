using Godot;
using Patchwork.Service;

namespace Patchwork.Scenes.Application;

public partial class Application : Control
{
	[Export] public string FirstPlayerName { get; set; } = "Player 1";
	[Export] public string SecondPlayerName { get; set; } = "Player 2";

	private RootService _rootService;

	// 它会在节点进入场景树并准备好后，被 Godot 自动调用
	public override void _Ready()
	{
		_rootService = new RootService();
		

		var gameScene = GetNode<Patchwork.Scenes.GameScene.GameScene>("GameScene");
		gameScene.Initialize(_rootService);
		
		_rootService.GameService.StartNewGame(FirstPlayerName, SecondPlayerName);
	}
}
