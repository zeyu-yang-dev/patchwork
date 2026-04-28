using Godot;
using Patchwork.Domain;
using Patchwork.Service;
using Patchwork.Scenes.GameScene;

namespace Patchwork.Scenes.Dashboard;

public partial class Dashboard : Control
{
	public DisplayTarget Target { get; set; } = DisplayTarget.CurrentPlayer;
	private RootService _rootService;
	private Label _nameLabel;
	private Label _moneyLabel;
	private Label _incomeLabel;
	private Label _scoreLabel;

	// =================================================================================================================
	
	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
		_rootService.StateChanged += OnGameStateChanged;
		
		_nameLabel = GetNode<Label>("NameLabel");
		_moneyLabel = GetNode<Label>("MoneyLabel");
		_incomeLabel = GetNode<Label>("IncomeLabel");
		_scoreLabel = GetNode<Label>("ScoreLabel");
	}

	// =================================================================================================================

	private Player GetTargetPlayer()
	{
		var currentGame = _rootService.CurrentGame;
		
		if (Target == DisplayTarget.CurrentPlayer)
		{
			return currentGame.CurrentPlayer;
		}
		else
		{
			var otherPlayerIndex = 1 - currentGame.CurrentPlayerIndex;
			return currentGame.Players[otherPlayerIndex];
		}
	}
	
	private void RefreshDashboard()
	{
		var player = GetTargetPlayer();
		
		_nameLabel.Text = player.Name;
		_moneyLabel.Text = player.Money.ToString();
		_incomeLabel .Text = player.Income.ToString();
		_scoreLabel.Text = player.Score.ToString();
	}
	
	private void OnGameStateChanged()
	{
		RefreshDashboard();
	}
}
