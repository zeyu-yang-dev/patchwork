using Godot;
using Patchwork.Domain;
using Patchwork.Service;
using System.Collections.Generic;

namespace Patchwork.Scenes.UnselectablePatches;

public partial class UnselectablePatches : Control
{
	private const string OriginalTextureDirectory = "res://Assets/Patches/original";
	private const float PatchSize = 50.0f;
	private const float PatchSpacing = 1.0f;
	private const int PatchDisplayAmount = 12;

	// For scroll functionality.
	private int _patchDisplayOffset = 0;
	private List<TextureRect> _displayNodes = [];
	
	private RootService _rootService;
	
	public override void _Ready()
	{
		for (var indexInDisplay = 0; indexInDisplay < PatchDisplayAmount; indexInDisplay++)
		{
			var displayNode = new TextureRect();
			
			displayNode.Size = new Vector2(PatchSize, PatchSize);
			displayNode.Position = new Vector2(
				indexInDisplay * (PatchSize + PatchSpacing),
				0
			);
			displayNode.Texture = GetPatchTexture(indexInDisplay);
			
			_displayNodes.Add(displayNode);
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
		var texture = ResourceLoader.Load<Texture2D>($"{OriginalTextureDirectory}/{id}.png");
		var image = texture.GetImage();
		image.Resize(Mathf.RoundToInt(PatchSize), Mathf.RoundToInt(PatchSize), Image.Interpolation.Lanczos);
		return ImageTexture.CreateFromImage(image);
	}
	
	private void RefreshVisual()
	{
		var unselectablePatches = _rootService.CurrentGame.PatchShop.GetUnselectablePatches();

		for (var indexInDisplay = 0; indexInDisplay < PatchDisplayAmount; indexInDisplay++)
		{
			// The index of the patch in unselectablePatches.
			var indexInShop = indexInDisplay + _patchDisplayOffset;
			
			if (indexInShop >= unselectablePatches.Count)
			{
				_displayNodes[indexInDisplay].Texture = null;
			}
			else
			{
				_displayNodes[indexInDisplay].Texture = GetPatchTexture(unselectablePatches[indexInShop].Id);
			}
		}
	}
	
	private void OnGameStateChanged()
	{
		RefreshVisual();
	}
}
