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
	
	public override void _Ready()
	{
		_rootService = new RootService();
		_rootService.GameEnded += OnGameEnded;
		
		_gameScene = GetNode<Patchwork.Scenes.GameScene.GameScene>("GameScene");
		_resultScene = GetNode<Patchwork.Scenes.ResultScene.ResultScene>("ResultScene");
		
		_gameScene.Initialize(_rootService);
		_resultScene.Initialize(_rootService);
		_resultScene._replayButton.Pressed += OnReplayButtonPressed;
		_resultScene._exitButton.Pressed += OnExitButtonPressed;
		
		_gameScene.Visible = true;
		_resultScene.Visible = false;
		
		_rootService.GameService.StartNewGame(FirstPlayerName, SecondPlayerName);
	}

	private void OnReplayButtonPressed()
	{
		_rootService.GameService.StartNewGame(FirstPlayerName, SecondPlayerName);
		
		_gameScene.Visible = true;
		_resultScene.Visible = false;
	}

	private void OnExitButtonPressed()
	{
		GetTree().Quit();
	}
	
	private void OnGameEnded()
	{
		_gameScene.Visible = true;
		_resultScene.Visible = true;
		_resultScene.MoveToFront();
	}
}
