using Godot;
using Patchwork.Domain;
using Patchwork.Service;

namespace Patchwork.Scenes.UnselectablePatches;

public partial class UnselectablePatches : Control
{
	private const string OriginalTextureDirectory = "res://Assets/Patches/original";
	private const float PatchSize = 50.0f;
	private const float PatchSpacing = 1.0f;
	private const int PatchDisplayAmount = 12;
	
	private int _patchDisplayOffset = 0;
	
	private RootService _rootService;
	
	
	public override void _Ready()
	{
		for (var indexInDisplay = 0; indexInDisplay < PatchDisplayAmount; indexInDisplay++)
		{
			var displayNode = new TextureRect();

			displayNode.Scale = new Vector2(
				PatchSize / 250.0f, 
				PatchSize / 250.0f
			);
			displayNode.Position = new Vector2(
				indexInDisplay * (PatchSize + PatchSpacing),
				0
			);
			displayNode.Texture = GetPatchTexture(indexInDisplay);
			
			AddChild(displayNode);
		}
		
	}
	
	public void Initialize(RootService rootService)
	{
		
		
		_rootService = rootService;
		_rootService.StateChanged += OnGameStateChanged;
	}

	public override void _GuiInput(InputEvent @event)
	{
		
	}



	private static Texture2D GetPatchTexture(int id)
	{
		return ResourceLoader.Load<Texture2D>($"{OriginalTextureDirectory}/{id}.png");
	}
	
	
	private void RefreshVisual()
	{
		var unselectablePatches = _rootService.CurrentGame.PatchShop.GetUnselectablePatches();

		for (var indexInDisplay = 0; indexInDisplay < PatchDisplayAmount; indexInDisplay++)
		{
			var indexInShop = indexInDisplay + _patchDisplayOffset;
			if (indexInShop >= unselectablePatches.Count) return;
		}
		
	}
	
	private void OnGameStateChanged()
	{
		RefreshVisual();
	}
}
