using Godot;
using Patchwork.Domain;
using Patchwork.Service;
using System;
using System.Linq;

namespace Patchwork.Scenes.ResultScene;

public partial class ResultScene : Control
{
	private RootService _rootService;
	private Label[][] _playerLabels;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var playersNode = GetNode<Node>("Window/MarginContainer/VBoxContainer/Players");

		_playerLabels = 
		[
			playersNode.GetNode<Node>("Player01").GetChildren().OfType<Label>().ToArray(),
			playersNode.GetNode<Node>("Player02").GetChildren().OfType<Label>().ToArray()
		];
		
		
	}

	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
		_rootService.GameEnded += OnGameEnded;
	}

	private void RefreshScoreBoard()
	{
		var players = _rootService.CurrentGame.Players;
		for (var i = 0; i < players.Count; i++)
		{
			var player = players[i];

			_playerLabels[i][0].Text = player.Name;
			_playerLabels[i][1].Text = player.Score.ToString();
			_playerLabels[i][2].Text = player.Money.ToString();
			_playerLabels[i][3].Text = player.PatchBoard.CountOccupiedCells().ToString();
			_playerLabels[i][4].Text = player.HasSevenBySevenBonus ? "Yes" : "No";
		}
	}

	
	
	
	private void OnGameEnded()
	{
		RefreshScoreBoard();
	}
}
