using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Patchwork.Service;

namespace Patchwork.Scenes.GameScene;

public partial class TimelineDisplay : Node2D
{
	private const float TokenStepSize = 32.0f;
	private const float TokenSpeed = 120.0f;
	private static readonly Vector2 SpecialPatchSpawnGlobal = new(525.0f, 555.0f);
	
	private RootService _rootService;
	private Sprite2D[] _tokens;
	private Sprite2D[] _specialPatches;
	
	// =================================================================================================================
	
	public override void _Ready()
	{
		_tokens =
		[
			GetNode<Sprite2D>("TimeToken01"),
			GetNode<Sprite2D>("TimeToken02"),
		];
		
		_specialPatches = GetNode<Node2D>("SpecialPatches").GetChildren().OfType<Sprite2D>().ToArray();
	}
	
	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
		_rootService.StateChanged += OnGameStateChanged;
		_rootService.GameStarted += OnGameStarted;
		_rootService.AdvanceStarted += OnAdvanceStarted;
		_rootService.IncomeChecked += OnIncomeChecked;
		_rootService.SpecialPatchChecked += OnSpecialPatchChecked;
	}

	// =================================================================================================================

	/// <summary>
	/// Resets the X position for both tokens.
	/// </summary>
	private void ResetTimeTokens()
	{
		foreach (var token in _tokens)
		{
			token.Position = new Vector2(0, token.Position.Y);
		}
	}

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
			var tokenTimePosition = Mathf.RoundToInt(_tokens[i].Position.X / TokenStepSize);
			
			if (playerTimePosition != tokenTimePosition) return i;
		}

		return null;
	}
	
	/// <summary>
	/// Moves a time token to the should-position with animation.
	/// </summary>
	private async Task PlayMoveTokenAnimation()
	{
		// The index of the target player and token.
		var targetIndex = GetTargetIndex();
		if (targetIndex == null) return;

		// The player-token pair. Only need to move for one pair.
		var player = _rootService.CurrentGame.Players[targetIndex.Value];
		var token = _tokens[targetIndex.Value];
		
		var targetX = player.TimePosition * TokenStepSize;
		var duration = Mathf.Abs(targetX - token.Position.X) / TokenSpeed;
		
		var tween = CreateTween();
		tween.TweenProperty(
			token, 
			"position:x", 
			targetX, 
			duration
		);
		// 等动画播放结束
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	private async Task PlayIncomeAnimation(List<int> incomePositions)
	{
		var tween = CreateTween();
		tween.TweenInterval(0.1f);
		await ToSignal(tween, Tween.SignalName.Finished);
	}
	
	private async Task PlaySpecialPatchAnimation(int? indexInFullArray)
	{
		if (indexInFullArray == null) return;
		var sprite = _specialPatches[indexInFullArray.Value];
		const float duration = 1.5f;
		
		var tween = CreateTween();
		tween.SetParallel();
		
		tween.TweenProperty(
			sprite, 
			"position", 
			SpecialPatchSpawnGlobal, 
			duration
		);
		tween.TweenProperty(
			sprite, 
			"scale", 
			new Vector2(1.0f, 1.0f), 
			duration
		);
		
		await ToSignal(tween, Tween.SignalName.Finished);
		
		sprite.QueueFree();
	}
	

	
	private async void OnAdvanceStarted(int startPosition, int targetPosition)
	{
		try
		{
			// 等待MoveTimeToken结束后再运行接下来的代码
			await PlayMoveTokenAnimation();
			_rootService.PlayerActionService.CheckForIncome(startPosition, targetPosition);
		}
		catch (Exception e)
		{
			GD.PrintErr(e);
		}
	}
	
	private async void OnIncomeChecked(List<int> incomePositions, int startPosition, int targetPosition)
	{
		try
		{
			await PlayIncomeAnimation(incomePositions);
			_rootService.PlayerActionService.CheckForSpecialPatch(startPosition, targetPosition);
		}
		catch (Exception e)
		{
			GD.PrintErr(e);
		}
		
	}
	
	private async void OnSpecialPatchChecked(int? indexInFullArray)
	{
		try
		{
			await PlaySpecialPatchAnimation(indexInFullArray);
			if (indexInFullArray != null)
			{
				_rootService.PatchService.TakeSpecialPatch();
			}
			else
			{
				_rootService.GameService.EndTurn();
			}
			
		}
		catch (Exception e)
		{
			GD.PrintErr(e);
		}
		
	}
	
	
	
	private void OnGameStateChanged() {}
	
	private void OnGameStarted()
	{
		ResetTimeTokens();
	}
	
	
}
