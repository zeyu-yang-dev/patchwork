using Godot;
using Patchwork.Domain;
using Patchwork.Service;

namespace Patchwork.Scenes.GameScene;

public partial class TimelineDisplay : Node2D
{
	private RootService _rootService;
	private Sprite2D[] _tokens;
	
	// =================================================================================================================
	
	public override void _Ready()
	{
		_tokens =
		[
			GetNode<Sprite2D>("TimeToken01"),
			GetNode<Sprite2D>("TimeToken02"),
		];

	}
	
	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
		_rootService.StateChanged += OnGameStateChanged;
	}

	// =================================================================================================================














	private void MoveTimeToken()
	{
		var currentGame =  _rootService.CurrentGame;
		var currentPlayer = currentGame.CurrentPlayer;
		var token = _tokens[currentGame.CurrentPlayerIndex];

	}
	
	
	// 由Service层驱动的刷新函数
	private void OnGameStateChanged()
	{
		MoveTimeToken();
	}
}
