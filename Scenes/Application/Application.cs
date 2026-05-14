using Godot;
using Patchwork.Service;

namespace Patchwork.Scenes.Application;

public partial class Application : Control
{
	[Export] public string FirstPlayerName { get; set; } = "Player 1";
	[Export] public string SecondPlayerName { get; set; } = "Player 2";

	private RootService _rootService;
	private Patchwork.Scenes.GameScene.GameScene _gameScene;
	private Patchwork.Scenes.ResultScene.ResultScene _resultScene;
	private Patchwork.Scenes.MainMenuScene.MainMenuScene _mainMenuScene;
	
	public override void _Ready()
	{
		_rootService = new RootService();
		
		_gameScene = GetNode<Patchwork.Scenes.GameScene.GameScene>("GameScene");
		_resultScene = GetNode<Patchwork.Scenes.ResultScene.ResultScene>("ResultScene");
		_mainMenuScene = GetNode<Patchwork.Scenes.MainMenuScene.MainMenuScene>("MainMenuScene");
		
		_gameScene.Initialize(_rootService);
		_resultScene.Initialize(_rootService);
		_mainMenuScene.Initialize(_rootService);
		
		_mainMenuScene.Visible = true;
		_gameScene.Visible = true;
		_resultScene.Visible = false;
	}
	
	
	
}
