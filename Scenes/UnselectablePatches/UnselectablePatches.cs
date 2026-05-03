using Godot;
using Patchwork.Domain;
using Patchwork.Service;

namespace Patchwork.Scenes.UnselectablePatches;

public partial class UnselectablePatches : Control
{
	private float _patchSize = 50.0f;
	private float _patchSpacing = 5.0f;
	private float _patchNumber;

	private int _patchOffset = 0;
	
	private RootService _rootService;
	
	
	public override void _Ready()
	{
		
	}
	
	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
		_rootService.StateChanged += OnGameStateChanged;
	}

	public override void _GuiInput(InputEvent @event)
	{
		
	}

	
	
	private void RefreshVisual()
	{
		
	}
	
	private void OnGameStateChanged()
	{
		RefreshVisual();
	}
}
