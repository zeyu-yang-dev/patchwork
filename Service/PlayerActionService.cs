using System;
using System.Collections.Generic;
using Patchwork.Domain;

namespace Patchwork.Service;

/// <summary>
/// Provides player action operations.
/// </summary>
public class PlayerActionService(RootService rootService)
{
    /// <summary>
    /// Advances the current player on the timeline and resolves all events along the path.
    /// </summary>
    /// <param name="steps">The number of steps to move forward.</param>
    public void Advance(int steps)
    {
        var currentGame = rootService.CurrentGame ?? throw new InvalidOperationException("There is no current game.");

        var currentPlayerIndex = currentGame.CurrentPlayerIndex;
        var currentPlayer = currentGame.CurrentPlayer;
        
        // Decides target position.
        var startPosition = currentPlayer.TimePosition;
        var targetPosition = Math.Min(startPosition + steps, currentGame.Timeline.EndPosition);
        
        // Marks the first player to reach the end point.
        if (targetPosition == currentGame.Timeline.EndPosition &&
            currentGame.FirstPlayerIndexToReachEnd == -1)
        {
            currentGame.FirstPlayerIndexToReachEnd = currentPlayerIndex;
        }
        
        // Moves the player's position directly to the destiny position.
        currentPlayer.TimePosition = targetPosition;
        // Refresh should take place before the current player get income or the special patch.
        rootService.NotifyAdvanceStarted(startPosition, targetPosition);
        
        // =============================================================================================================
        
        // Checks whether income positions are reached.
        foreach (var incomePosition in currentGame.Timeline.IncomePositions)
        {
            if (incomePosition > startPosition && incomePosition <= targetPosition)
            {
                currentPlayer.Money += currentPlayer.Income;
            }
        }
        
        // =============================================================================================================
        
        // Checks whether any of the special patch positions is reached.
        // Can't handle multiple special patches, which is not possible to happen.
        for (var i = currentGame.Timeline.RemainingSpecialPatchPositions.Count - 1; i >= 0; i--)
        {
            var specialPatchPosition = currentGame.Timeline.RemainingSpecialPatchPositions[i];
            if (specialPatchPosition > startPosition && specialPatchPosition <= targetPosition)
            {
                currentGame.Timeline.RemainingSpecialPatchPositions.RemoveAt(i);
                
                rootService.PatchService.TakeSpecialPatch();
            }
        }
        
        
        
        rootService.GameService.EndTurn();
    }
    
    
    
    /// <summary>
    /// 1. Step of advance action: Increase the time position of the current player.
    /// </summary>
    private (int startPosition, int targetPosition) StartAdvance(int steps)
    {
        var currentGame = rootService.CurrentGame ?? throw new InvalidOperationException("There is no current game.");
        var currentPlayer = currentGame.CurrentPlayer;
        var currentPlayerIndex = currentGame.CurrentPlayerIndex;
        
        // Decides target position.
        var startPosition = currentPlayer.TimePosition;
        var targetPosition = Math.Min(startPosition + steps, currentGame.Timeline.EndPosition);
        
        // Marks the first player to reach the end point.
        if (targetPosition == currentGame.Timeline.EndPosition &&
            currentGame.FirstPlayerIndexToReachEnd == -1)
        {
            currentGame.FirstPlayerIndexToReachEnd = currentPlayerIndex;
        }
        
        // Moves the player's position directly to the destiny position.
        currentPlayer.TimePosition = targetPosition;
        
        // Refresh should take place before the current player get income or the special patch.
        rootService.NotifyAdvanceStarted(startPosition, targetPosition);
        return (startPosition, targetPosition);
    }

    /// <summary>
    /// 2. Step of advance action: Check for income positions.
    /// </summary>
    public List<int> CheckForIncome(int startPosition, int targetPosition)
    {
        var currentGame = rootService.CurrentGame ?? throw new InvalidOperationException("There is no current game.");
        var currentPlayer = currentGame.CurrentPlayer;

        var incomeIndices = new List<int>();
        // Checks whether income positions are reached.
        for (var incomeIndex = 0; incomeIndex < currentGame.Timeline.IncomePositions.Count; incomeIndex++)
        {
            var incomePosition = currentGame.Timeline.IncomePositions[incomeIndex];
            if (startPosition < incomePosition && incomePosition <= targetPosition)
            {
                currentPlayer.Money += currentPlayer.Income;
                incomeIndices.Add(incomeIndex);
            }
        }
        
        rootService.NotifyIncomeChecked(incomeIndices, startPosition, targetPosition);
        return incomeIndices;
    }

    /// <summary>
    /// 3. Step of advance action:
    /// </summary>
    public int? CheckForSpecialPatch(int startPosition, int targetPosition)
    {
        var currentGame = rootService.CurrentGame ?? throw new InvalidOperationException("There is no current game.");
        var remainingPositions = currentGame.Timeline.RemainingSpecialPatchPositions;
        const int totalNumber = 5;
        var remainingNumber = remainingPositions.Count;

        
        for (int i = totalNumber - remainingNumber; i < totalNumber; i++)
        {
            var indexInRemaining = i - (totalNumber - remainingNumber);
            var specialPatchPosition = remainingPositions[indexInRemaining];
            if (startPosition < specialPatchPosition && specialPatchPosition <= targetPosition)
            {
                remainingPositions.RemoveAt(indexInRemaining);
                rootService.NotifySpecialPatchChecked(i);
                return i;
            }
        }
        
        rootService.NotifySpecialPatchChecked(null);
        return null;
    }
    
    
    
    
    
    
    
    
    // =================================================================================================================
    
    public void Skip()
    {
        var currentGame = rootService.CurrentGame ?? throw new InvalidOperationException("There is no current game.");

        if (currentGame.CurrentPlacedPatch != null)
        {
            throw new InvalidOperationException("Unable to skip when a patch is selected.");
        }

        var currentPlayer = currentGame.CurrentPlayer;
        var otherPlayerIndex = currentGame.CurrentPlayerIndex == 0 ? 1 : 0;
        var otherPlayer = currentGame.Players[otherPlayerIndex];

        var targetPosition = Math.Min(otherPlayer.TimePosition + 1, currentGame.Timeline.EndPosition);
        var steps = targetPosition - currentPlayer.TimePosition;
        
        currentPlayer.Money += steps;
        StartAdvance(steps);
        
        // Notify UI to refresh
        rootService.NotifyStateChanged();
    }
    
    // =================================================================================================================

    private void BuyPatch()
    {
        var currentGame = rootService.CurrentGame ?? throw new InvalidOperationException("There is no current game.");
        var currentPlacedPatch = currentGame.CurrentPlacedPatch ?? throw new InvalidOperationException("There is no patch waiting to be bought.");
        
        // Skip if the current patch is the special patch.
        if (currentPlacedPatch.Patch.Id == -1) return;
        
        var currentPlayer = currentGame.CurrentPlayer;
        var selectablePatches = currentGame.PatchShop.GetSelectablePatches();
        var patchOffset = selectablePatches.FindIndex(patch => patch.Id == currentPlacedPatch.Patch.Id);
        
        // Removes the path from the patch shop.
        var patch = currentGame.PatchShop.RemovePatch(patchOffset);
        
        // The player pay the price of the patch.
        currentPlayer.Money -= patch.MoneyCost;
        
        currentGame.CurrentPlacedPatch = null;
        
        // Notify UI to refresh
        rootService.NotifyStateChanged();
    }
    
    /// <summary>
    /// Gets the offsets of the buyable patches that the current player can both afford and place somewhere on the board.
    /// </summary>
    /// <returns>A list of buyable patch offsets, a subset of {0, 1, 2}.</returns>
    public List<int> GetBuyablePatchOffsets()
    {
        var currentGame = rootService.CurrentGame ?? throw new InvalidOperationException("There is no current game.");

        var currentPlayer = currentGame.CurrentPlayer;
        var selectablePatches = currentGame.PatchShop.GetSelectablePatches();
        var buyablePatchOffsets = new List<int>();

        for (var offset = 0; offset < selectablePatches.Count; offset++)
        {
            // a patch is buyable, if the player has enough money and the patch is placeable
            if (selectablePatches[offset].MoneyCost <= currentPlayer.Money &&
                currentPlayer.PatchBoard.IsPlaceable(selectablePatches[offset]))
            {
                buyablePatchOffsets.Add(offset);
            }
        }

        return buyablePatchOffsets;
    }

    // =================================================================================================================

    public bool IsPlaceable()
    {
        var currentGame = rootService.CurrentGame ?? throw new InvalidOperationException("There is no current game.");
        var currentPlayer = currentGame.CurrentPlayer ?? throw new InvalidOperationException("There is no current player");
        var currentPlacedPatch = currentGame.CurrentPlacedPatch ?? throw new InvalidOperationException("There is no patch waiting to be placed.");
        var coordinate = currentPlacedPatch.Coordinate;
        
        // The patch is not placeable, if it doesn't have a coordinate.
        if (coordinate == null) return false;
        
        return currentPlayer.PatchBoard.IsPlaceable(currentPlacedPatch, coordinate.Value.col, coordinate.Value.row);
    }
    
    public void PlacePatch()
    {
        if (!IsPlaceable())
        {
            throw new InvalidOperationException("The current patch cannot be placed.");
        }

        var currentGame = rootService.CurrentGame ?? throw new InvalidOperationException("There is no current game.");
        var currentPlacedPatch = currentGame.CurrentPlacedPatch ?? throw new InvalidOperationException("There is no patch waiting to be placed.");
        var coordinate = currentPlacedPatch.Coordinate ?? throw new InvalidOperationException("The current patch does not contain a coordinate.");
        var currentPlayer = currentGame.CurrentPlayer ?? throw new InvalidOperationException("There is no current player");

        currentPlayer.PatchBoard.PlacePatch(currentPlacedPatch, coordinate.col, coordinate.row);
        
        // Checks whether the player should be awarded with the bonus.
        if (!currentGame.Players[0].HasSevenBySevenBonus &&
            !currentGame.Players[1].HasSevenBySevenBonus &&
            currentPlayer.PatchBoard.HasFilled7x7Area())
        {
            currentPlayer.HasSevenBySevenBonus = true;
        }
        
        // Notify UI to refresh
        rootService.NotifyStateChanged();

        if (currentPlacedPatch.Patch.Id != -1)
        {
            BuyPatch();
        }
        else
        {
            currentGame.CurrentPlacedPatch = null;
        }
        
        
        
        // Advance always takes place after placing a patch.
        StartAdvance(currentPlacedPatch.Patch.TimeCost);
    }
}
