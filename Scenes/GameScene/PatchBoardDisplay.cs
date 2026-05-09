using Godot;
using Patchwork.Domain;
using Patchwork.Service;

namespace Patchwork.Scenes.GameScene;

public enum DisplayTarget
{
	CurrentPlayer,
	OtherPlayer
}

// 凡是派生自 GodotObject 的类型，都应该声明为 partial
public partial class PatchBoardDisplay : Node2D
{
	public DisplayTarget Target { get; set; } = DisplayTarget.CurrentPlayer;
	private float CellSize => Target == DisplayTarget.CurrentPlayer ? 50.0f : 20.0f;
	
	private const int MatrixSize = 5;
	private const string OriginalTextureDirectory = "res://Assets/GameScene/Patches/original";
	private const string MirroredTextureDirectory = "res://Assets/GameScene/Patches/mirrored";
	
	private RootService _rootService;

	// =================================================================================================================
	
	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
		_rootService.StateChanged += OnGameStateChanged;
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

	private Vector2 GetPatchCenterPosition(PlacedPatch placedPatch)
	{
		var coordinate = placedPatch.Coordinate
						 ?? throw new System.InvalidOperationException("PlacedPatch.Coordinate is null.");
		return new Vector2(
			(coordinate.col + 0.5f) * CellSize,
			(coordinate.row + 0.5f) * CellSize
		);
	}

	private static Texture2D GetPatchTexture(PlacedPatch placedPatch)
	{
		var directory = placedPatch.IsMirrored ? MirroredTextureDirectory : OriginalTextureDirectory;
		return ResourceLoader.Load<Texture2D>($"{directory}/{placedPatch.Patch.Id}.png");
	}

	private void DisplaySinglePlacedPatch(PlacedPatch placedPatch)
	{
		var sprite = new Sprite2D();
		AddChild(sprite);

		var texture = GetPatchTexture(placedPatch);
		sprite.Texture = texture;
		
		// 让Sprite2D以目标大小缩放
		sprite.Scale = new Vector2(
			CellSize * MatrixSize / texture.GetSize().X, 
			CellSize * MatrixSize / texture.GetSize().Y
		);
		
		// 让Sprite2D以中心点作为定位基准
		sprite.Centered = true;
		sprite.Position = GetPatchCenterPosition(placedPatch);
		sprite.Rotation = Mathf.DegToRad(placedPatch.Rotation);
	}
	
	private void RefreshVisual()
	{
		foreach (var child in GetChildren())
		{
			child.QueueFree();
		}
		
		foreach (var placedPatch in GetTargetPlayer().PatchBoard.PlacedPatches)
		{
			DisplaySinglePlacedPatch(placedPatch);
		}
	}
	
	// 由Service层驱动的刷新函数
	private void OnGameStateChanged()
	{
		RefreshVisual();
	}
}
