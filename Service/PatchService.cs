using System;
using Patchwork.Domain;

namespace Patchwork.Service;

public class PatchService(RootService rootService)
{
    /// <summary>
    /// Prepares the selected patch for subsequent transformation and placement.
    /// Invoked when a patch is dragged from patch shop to patch board.
    /// </summary>
    public void TakePatch(int patchOffset)
    {
        var currentGame = rootService.CurrentGame
                          ?? throw new InvalidOperationException("There is no current game.");

        if (currentGame.CurrentPlacedPatch != null)
        {
            throw new InvalidOperationException("There is already a patch waiting to be placed.");
        }

        var buyablePatchOffsets = rootService.PlayerActionService.GetBuyablePatchOffsets();

        if (!buyablePatchOffsets.Contains(patchOffset))
        {
            throw new InvalidOperationException("The selected patch cannot be taken by the current player.");
        }

        var patch = currentGame.PatchShop.GetSelectablePatches()[patchOffset];
        currentGame.CurrentPlacedPatch = new PlacedPatch(patch);
        
        // Notify UI to refresh
        rootService.NotifyStateChanged();
    }

    
    /// <summary>
    /// The inverse operation of <see cref="TakePatch"/>.
    /// </summary>
    public void PutBackPatch()
    {
        var currentGame = rootService.CurrentGame
                          ?? throw new InvalidOperationException("There is no current game.");

        if (currentGame.CurrentPlacedPatch == null)
        {
            throw new InvalidOperationException("There is no patch waiting to be put back.");
        }

        currentGame.CurrentPlacedPatch = null;
        
        // Notify UI to refresh
        rootService.NotifyStateChanged();
    }
    
    // =================================================================================================================

    public void MovePatch(int col, int row)
    {
        var currentPlacedPatch = GetCurrentPlacedPatch();
        currentPlacedPatch.Coordinate = (col, row);
        
        // Notify UI to refresh
        rootService.NotifyStateChanged();
    }
    
    public void RotateClockwise()
    {
        var currentPlacedPatch = GetCurrentPlacedPatch();
        var targetShape = PlacedPatch.RotateShape90(currentPlacedPatch.GetCurrentShape());
        ApplyTransformation(targetShape);
        
        // Notify UI to refresh
        rootService.NotifyStateChanged();
    }

    public void RotateCounterClockwise()
    {
        var currentPlacedPatch = GetCurrentPlacedPatch();
        var currentShape = currentPlacedPatch.GetCurrentShape();
        var targetShape = PlacedPatch.RotateShape90(PlacedPatch.RotateShape90(PlacedPatch.RotateShape90(currentShape)));
        ApplyTransformation(targetShape);
        
        // Notify UI to refresh
        rootService.NotifyStateChanged();
    }

    public void Mirror()
    {
        var currentPlacedPatch = GetCurrentPlacedPatch();
        var targetShape = PlacedPatch.MirrorShape(currentPlacedPatch.GetCurrentShape());
        ApplyTransformation(targetShape);
        
        // Notify UI to refresh
        rootService.NotifyStateChanged();
    }

    // =================================================================================================================
    
    private PlacedPatch GetCurrentPlacedPatch()
    {
        var currentGame = rootService.CurrentGame
                          ?? throw new InvalidOperationException("There is no current game.");

        return currentGame.CurrentPlacedPatch
               ?? throw new InvalidOperationException("There is no patch waiting to be transformed.");
    }
    
    /// <summary>
    /// Change the isMirrored and rotation properties of the current placed patch,
    /// so that it has the target shape.
    /// </summary>
    private void ApplyTransformation(bool[,] targetShape)
    {
        var currentPlacedPatch = GetCurrentPlacedPatch();
        var (isMirrored, rotation) = MatchTransformation(targetShape);

        currentPlacedPatch.IsMirrored = isMirrored;
        currentPlacedPatch.Rotation = rotation;
    }

    private (bool isMirrored, int rotation) MatchTransformation(bool[,] targetShape)
    {
        var currentPlacedPatch = GetCurrentPlacedPatch();

        foreach (var isMirrored in new[] { false, true })
        {
            foreach (var rotation in new[] { 0, 90, 180, 270 })
            {
                var candidate = new PlacedPatch(currentPlacedPatch.Patch)
                {
                    IsMirrored = isMirrored,
                    Rotation = rotation
                };

                if (AreShapesEqual(candidate.GetCurrentShape(), targetShape))
                {
                    return (isMirrored, rotation);
                }
            }
        }

        throw new InvalidOperationException("Unable to match the target transformation.");
    }

    private static bool AreShapesEqual(bool[,] firstShape, bool[,] secondShape)
    {
        for (var row = 0; row < 5; row++)
        {
            for (var col = 0; col < 5; col++)
            {
                if (firstShape[row, col] != secondShape[row, col])
                {
                    return false;
                }
            }
        }

        return true;
    }
}
