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
	private readonly List<TextureRect> _displayNodes = [];
	
	private RootService _rootService;
	
	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Stop;
		MouseExited += OnMouseExited;
		
		for (var indexInDisplay = 0; indexInDisplay < PatchDisplayAmount; indexInDisplay++)
		{
			var displayNode = new TextureRect();
			
			displayNode.Size = new Vector2(PatchSize, PatchSize);
			displayNode.Position = new Vector2(
				indexInDisplay * (PatchSize + PatchSpacing),
				0
			);
			displayNode.Texture = GetPatchTexture(indexInDisplay);
			displayNode.MouseFilter = MouseFilterEnum.Ignore;
			
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
		if (@event is not InputEventMouseButton { Pressed: true } mouseButton)
		{
			return;
		}

		if (mouseButton.ButtonIndex is not MouseButton.WheelUp and not MouseButton.WheelDown)
		{
			return;
		}

		if (mouseButton.ButtonIndex == MouseButton.WheelUp)
		{
			_patchDisplayOffset = Mathf.Max(0, _patchDisplayOffset - 1);
		}
		else
		{
			_patchDisplayOffset += 1;
		}

		RefreshVisual();
		AcceptEvent();
	}
	
	/// <summary>
	/// Resets _patchDisplayOffset when the cursor lefts this area.
	/// </summary>
	private void OnMouseExited()
	{
		_patchDisplayOffset = 0;
		RefreshVisual();
	}
	
	/// <summary>
	/// Returns a resized patch texture.
	/// </summary>
	private static Texture2D GetPatchTexture(int id)
	{
		var texture = ResourceLoader.Load<Texture2D>($"{OriginalTextureDirectory}/{id}.png");
		var image = texture.GetImage();
		image.Resize(Mathf.RoundToInt(PatchSize), Mathf.RoundToInt(PatchSize), Image.Interpolation.Lanczos);
		return ImageTexture.CreateFromImage(image);
	}
	
	/// <summary>
	/// Refreshes the displayed patches with relation to _patchDisplayOffset.
	/// </summary>
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
