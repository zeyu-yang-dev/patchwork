using System;
using Godot;
using Patchwork.Service;

namespace Patchwork.Scenes.PatchBoard;

public partial class PatchBoard : Panel
{
	public event Action DragCancelled;

	private enum InteractionState
	{
		Idle,
		DraggingFromShop,
		PlacingPatch,
		DraggingPlacedPatch
	}

	private const int BoardSize = 9;
	private const float BoardPixelSize = 450.0f;
	private const float BoardCellSize = BoardPixelSize / BoardSize;

	private RootService _rootService;
	private Control _placedPatchLayer;
	private Patchwork.Scenes.ActivePatchView.ActivePatchView _activePatchView;
	private InteractionState _interactionState = InteractionState.Idle;
	private Vector2 _dragOffsetFromPatchCenter;

	public override void _Ready()
	{
		_placedPatchLayer = new Control
		{
			Name = "PlacedPatchLayer",
			MouseFilter = MouseFilterEnum.Ignore
		};
		_placedPatchLayer.SetAnchorsPreset(LayoutPreset.FullRect);
		AddChild(_placedPatchLayer);

		_activePatchView = new Patchwork.Scenes.ActivePatchView.ActivePatchView
		{
			Name = "ActivePatchView",
			Visible = false
		};
		_activePatchView.DragStarted += OnActivePatchViewDragStarted;
		AddChild(_activePatchView);
	}

	public override void _Process(double delta)
	{
		if (_interactionState is not (InteractionState.DraggingFromShop or InteractionState.DraggingPlacedPatch))
		{
			return;
		}

		var patchCenterGlobal = GetGlobalMousePosition() - _dragOffsetFromPatchCenter;

		if (_interactionState == InteractionState.DraggingFromShop)
		{
			UpdateFreeDragPosition(patchCenterGlobal);

			if (ContainsGlobalPoint(patchCenterGlobal))
			{
				_interactionState = InteractionState.PlacingPatch;
				UpdatePlacementFromPatchCenter(patchCenterGlobal);
			}

			return;
		}

		UpdatePlacementFromPatchCenter(patchCenterGlobal);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false })
		{
			return;
		}

		switch (_interactionState)
		{
			case InteractionState.DraggingFromShop:
				_rootService.PatchService.PutBackPatch();
				CancelDrag();
				DragCancelled?.Invoke();
				_interactionState = InteractionState.Idle;
				break;
			case InteractionState.DraggingPlacedPatch:
				_interactionState = InteractionState.PlacingPatch;
				break;
		}
	}

	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
		Refresh();
	}

	public void BeginDragFromShop(int patchOffset, Vector2 sourceCenterGlobal, Vector2 dragOffsetFromCenter)
	{
		if (_rootService?.CurrentGame?.CurrentPlacedPatch != null)
		{
			return;
		}

		_rootService.PatchService.TakePatch(patchOffset);
		_dragOffsetFromPatchCenter = dragOffsetFromCenter;
		_interactionState = InteractionState.DraggingFromShop;
		_activePatchView.DisplayPlacedPatch(_rootService.CurrentGame.CurrentPlacedPatch);
		_activePatchView.Position = GetLocalPositionFromGlobalCenter(sourceCenterGlobal);
		_activePatchView.Modulate = Colors.White;
		_activePatchView.Visible = true;
	}

	public void Refresh()
	{
		if (_rootService?.CurrentGame == null)
		{
			return;
		}

		foreach (var child in _placedPatchLayer.GetChildren())
		{
			child.QueueFree();
		}

		foreach (var placedPatch in _rootService.CurrentGame.CurrentPlayer.PatchBoard.PlacedPatches)
		{
			var activePatchView = new Patchwork.Scenes.ActivePatchView.ActivePatchView
			{
				MouseFilter = MouseFilterEnum.Ignore
			};

			_placedPatchLayer.AddChild(activePatchView);
			activePatchView.DisplayPlacedPatch(placedPatch);
			activePatchView.Position = GetPatchTopLeftFromCenter(placedPatch.Coordinate!.Value.col, placedPatch.Coordinate!.Value.row);
		}
	}

	public bool ContainsGlobalPoint(Vector2 globalPoint)
	{
		return GetGlobalRect().HasPoint(globalPoint);
	}

	public void CancelDrag()
	{
		var currentPlacedPatch = _rootService?.CurrentGame?.CurrentPlacedPatch;

		if (currentPlacedPatch != null)
		{
			currentPlacedPatch.Coordinate = null;
		}

		_activePatchView.Clear();
		_activePatchView.Visible = false;
		_activePatchView.Modulate = Colors.White;
	}

	private void UpdateFreeDragPosition(Vector2 patchCenterGlobal)
	{
		_activePatchView.Position = GetLocalPositionFromGlobalCenter(patchCenterGlobal);
		_activePatchView.Modulate = Colors.White;
		_activePatchView.Visible = true;
	}

	private void UpdatePlacementFromPatchCenter(Vector2 patchCenterGlobal)
	{
		var currentPlacedPatch = _rootService?.CurrentGame?.CurrentPlacedPatch;

		if (currentPlacedPatch == null)
		{
			CancelDrag();
			return;
		}

		var boardLocalCenter = GetGlobalTransform().AffineInverse() * patchCenterGlobal;
		var col = Mathf.FloorToInt(boardLocalCenter.X / BoardCellSize);
		var row = Mathf.FloorToInt(boardLocalCenter.Y / BoardCellSize);

		if (!IsInsideBoard(col, row))
		{
			return;
		}

		_rootService.PatchService.MovePatch(col, row);
		_activePatchView.Position = GetPatchTopLeftFromCenter(col, row);
		_activePatchView.Modulate = IsCurrentPatchPlaceable(col, row)
			? Colors.White
			: new Color(1.0f, 1.0f, 1.0f, 0.35f);
		_activePatchView.Visible = true;
	}

	private bool IsCurrentPatchPlaceable(int col, int row)
	{
		var currentGame = _rootService.CurrentGame;
		var currentPlacedPatch = currentGame.CurrentPlacedPatch;
		return currentGame.CurrentPlayer.PatchBoard.IsPlaceable(currentPlacedPatch, col, row);
	}

	private static bool IsInsideBoard(int col, int row)
	{
		return col >= 0 && col < BoardSize && row >= 0 && row < BoardSize;
	}

	private static Vector2 GetPatchTopLeftFromCenter(int col, int row)
	{
		var cellCenter = new Vector2((col + 0.5f) * BoardCellSize, (row + 0.5f) * BoardCellSize);
		return cellCenter - Patchwork.Scenes.ActivePatchView.ActivePatchView.CenterOffset;
	}

	private Vector2 GetLocalPositionFromGlobalCenter(Vector2 sourceCenterGlobal)
	{
		var patchTopLeftGlobal = sourceCenterGlobal - Patchwork.Scenes.ActivePatchView.ActivePatchView.CenterOffset;
		return GetGlobalTransform().AffineInverse() * patchTopLeftGlobal;
	}

	private void OnActivePatchViewDragStarted(Patchwork.Scenes.ActivePatchView.ActivePatchView _, Vector2 dragOffsetFromCenter)
	{
		if (_interactionState != InteractionState.PlacingPatch)
		{
			return;
		}

		_dragOffsetFromPatchCenter = dragOffsetFromCenter;
		_interactionState = InteractionState.DraggingPlacedPatch;
	}
}
