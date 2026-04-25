using Godot;
using Patchwork.Domain;
using Patchwork.Service;

namespace Patchwork.Scenes.GameScene;

public partial class TimelineDisplay : Node2D
{
	private const float StepSize = 32.0f;
	private const float Speed = 90.0f;
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


	/// <summary>
	/// Returns the index of the player and the token, for which the token should be moved.
	/// </summary>
	/// <returns>Index of the player and the corresponding token. Null if no need to move.</returns>
	private int? GetTargetIndex()
	{
		var currentGame = _rootService.CurrentGame;
		for (var i = 0; i < _tokens.Length; i++)
		{
			var player = currentGame.Players[i];
			// Read from GameState.
			var playerTimePosition = player.TimePosition;
			// The actual value.
			var tokenTimePosition = Mathf.RoundToInt(_tokens[i].Position.X / StepSize);
			
			if (playerTimePosition != tokenTimePosition) return i;
		}

		return null;
	}
	
	private void MoveTimeToken()
	{
		// The index of the target player and token.
		var targetIndex = GetTargetIndex();
		if (targetIndex == null) return;

		// The player-token pair. Only need to move for one pair.
		var player = _rootService.CurrentGame.Players[targetIndex.Value];
		var token = _tokens[targetIndex.Value];
		
		var targetX = player.TimePosition * StepSize;
		var duration = Mathf.Abs(targetX - token.Position.X) / Speed;
		
		var tween = CreateTween();
		tween.TweenProperty(
			token, 
			"position:x", 
			targetX, 
			duration
		);




	}
	
	
	// 由Service层驱动的刷新函数
	private void OnGameStateChanged()
	{
		MoveTimeToken();
	}
}
