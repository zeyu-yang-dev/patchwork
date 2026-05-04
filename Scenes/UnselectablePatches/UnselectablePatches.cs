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
	private const float PriceTagHeight = 20.0f;

	// For scroll functionality.
	private int _patchDisplayOffset = 0;
	private readonly List<TextureRect> _displayNodes = [];
	private readonly List<Label> _priceTagNodes = [];
	private Label _priceTagBackground;
	
	private RootService _rootService;
	
	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Stop;
		MouseExited += OnMouseExited;
		MouseEntered += OnMouseEntered;
		
		// Initialize nodes for patch display.
		for (var indexInDisplay = 0; indexInDisplay < PatchDisplayAmount; indexInDisplay++)
		{
			var displayNode = new TextureRect();
			displayNode.MouseFilter = MouseFilterEnum.Ignore;
			
			displayNode.Size = new Vector2(PatchSize, PatchSize);
			displayNode.Position = new Vector2(indexInDisplay * (PatchSize + PatchSpacing), 0);
			displayNode.Texture = GetPatchTexture(indexInDisplay); // Initial display content.
			
			_displayNodes.Add(displayNode);
			AddChild(displayNode);
		}
		
		// Initialize a node to display background for price tags.
		var backgroundNode = new Label();
		backgroundNode.MouseFilter = MouseFilterEnum.Ignore;
		backgroundNode.Size = new Vector2(Size.X, PriceTagHeight);
		backgroundNode.Position = new Vector2(0, PatchSize);
		backgroundNode.AddThemeStyleboxOverride("normal",  CreateLabelBackground());
		_priceTagBackground = backgroundNode;
		AddChild(backgroundNode);
		
		// Initialize nodes for price tags.
		for (var indexInDisplay = 0; indexInDisplay < PatchDisplayAmount; indexInDisplay++)
		{
			var priceTagNode = new Label();
			priceTagNode.MouseFilter = MouseFilterEnum.Ignore;
			
			priceTagNode.Size = new Vector2(PatchSize, PriceTagHeight);
			priceTagNode.Position = new Vector2(indexInDisplay * (PatchSize + PatchSpacing), PatchSize);
			priceTagNode.HorizontalAlignment = HorizontalAlignment.Center;
			priceTagNode.VerticalAlignment = VerticalAlignment.Center;
			priceTagNode.AddThemeColorOverride("font_color", new Color(0.95f, 0.95f, 0.95f, 1.0f));
			priceTagNode.Text = "9/9"; // Initial display content.
			
			_priceTagNodes.Add(priceTagNode);
			AddChild(priceTagNode);
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

	private void OnMouseEntered()
	{
		
		RefreshVisual();
	}
	
	/// <summary>
	/// Helper function to create a style box to display background.
	/// </summary>
	private static StyleBoxFlat CreateLabelBackground()
	{
		var background = new StyleBoxFlat();
		background.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
		background.SetCornerRadiusAll(3);
		return background;
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
