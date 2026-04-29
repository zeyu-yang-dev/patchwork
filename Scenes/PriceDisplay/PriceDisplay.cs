using Godot;
using Patchwork.Service;

namespace Patchwork.Scenes.PriceDisplay;

public partial class PriceDisplay : Control
{
	private RootService _rootService;
	private Label[] _moneyCostLabels;
	private Label[] _timeCostLabels;

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;

		_moneyCostLabels =
		[
			GetNode<Label>("PriceTag01/MoneyCost"),
			GetNode<Label>("PriceTag02/MoneyCost"),
			GetNode<Label>("PriceTag03/MoneyCost")
		];

		_timeCostLabels =
		[
			GetNode<Label>("PriceTag01/TimeCost"),
			GetNode<Label>("PriceTag02/TimeCost"),
			GetNode<Label>("PriceTag03/TimeCost")
		];
	}

	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
		_rootService.StateChanged += OnGameStateChanged;
	}

	private void RefreshTags()
	{
		var selectablePatches = _rootService.CurrentGame.PatchShop.GetSelectablePatches();

		for (var i = 0; i < 3; i++)
		{
			if (i < selectablePatches.Count)
			{
				_moneyCostLabels[i].Visible = true;
				_timeCostLabels[i].Visible = true;
				_moneyCostLabels[i].Text = selectablePatches[i].MoneyCost.ToString();
				_timeCostLabels[i].Text = selectablePatches[i].TimeCost.ToString();
			}
			else
			{
				_moneyCostLabels[i].Visible = false;
				_timeCostLabels[i].Visible = false;
			}
		}
	}

	private void OnGameStateChanged()
	{
		RefreshTags();
	}
}
