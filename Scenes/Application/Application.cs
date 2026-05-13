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
		_rootService.GameEnded += OnGameEnded;
		
		_gameScene = GetNode<Patchwork.Scenes.GameScene.GameScene>("GameScene");
		_resultScene = GetNode<Patchwork.Scenes.ResultScene.ResultScene>("ResultScene");
		_mainMenuScene = GetNode<Patchwork.Scenes.MainMenuScene.MainMenuScene>("MainMenuScene");
		
		_gameScene.Initialize(_rootService);
		_resultScene.Initialize(_rootService);
		_resultScene._replayButton.Pressed += OnReplayButtonPressed;
		_resultScene._exitButton.Pressed += OnExitButtonPressed;
		_mainMenuScene.Initialize(_rootService);
		_mainMenuScene._startButton.Pressed += OnStartButtonPressed;
		_mainMenuScene._exitButton.Pressed += OnExitButtonPressed;
		
		_mainMenuScene.Visible = true;
		_gameScene.Visible = true;
		_resultScene.Visible = false;
		
		// _rootService.GameService.StartNewGame(FirstPlayerName, SecondPlayerName);
	}
	
	private void OnGameEnded()
	{
		_gameScene.Visible = true;
		
		_resultScene.Visible = true;
		_resultScene.Modulate = new Color(1, 1, 1, 0);
		
		_resultScene.MoveToFront();
		
		var tween = CreateTween();
		tween.TweenProperty(_resultScene, "modulate:a", 1.0f, 0.5f);
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

	private void OnStartButtonPressed()
	{
		_rootService.GameService.StartNewGame(FirstPlayerName, SecondPlayerName);
	}
}
