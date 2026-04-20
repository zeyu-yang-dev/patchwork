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
        var currentGame = rootService.CurrentGame
            ?? throw new InvalidOperationException("There is no current game.");

        var currentPlayerIndex = currentGame.CurrentPlayerIndex;
        var currentPlayer = currentGame.CurrentPlayer;
        var otherPlayerIndex = currentPlayerIndex == 0 ? 1 : 0;
        var otherPlayer = currentGame.Players[otherPlayerIndex];

        // Decides end position.
        var startPosition = currentPlayer.TimePosition;
        var destinyPosition = Math.Min(startPosition + steps, currentGame.Timeline.EndPosition);

        // Checks whether income positions are reached.
        foreach (var incomePosition in currentGame.Timeline.IncomePositions)
        {
            if (incomePosition > startPosition && incomePosition <= destinyPosition)
            {
                currentPlayer.Money += currentPlayer.Income;
            }
        }

        // Checks whether special patch positions are reached.
        for (var i = currentGame.Timeline.RemainingSpecialPatchPositions.Count - 1; i >= 0; i--)
        {
            var specialPatchPosition = currentGame.Timeline.RemainingSpecialPatchPositions[i];
            if (specialPatchPosition > startPosition && specialPatchPosition <= destinyPosition)
            {
                currentGame.Timeline.RemainingSpecialPatchPositions.RemoveAt(i);
                currentGame.CurrentPlacedPatch = new PlacedPatch(new Patch(-1));
            }
        }

        // Moves the player's position directly to the destiny position.
        currentPlayer.TimePosition = destinyPosition;

        // Marks the first player to reach the end point.
        if (destinyPosition == currentGame.Timeline.EndPosition &&
            currentGame.FirstPlayerIndexToReachEnd == -1)
        {
            currentGame.FirstPlayerIndexToReachEnd = currentPlayerIndex;
        }

        // Decides the next player in turn.
        currentGame.CurrentPlayerIndex =
            currentPlayer.TimePosition > otherPlayer.TimePosition
                ? otherPlayerIndex
                : currentPlayerIndex;
    }

    /// <summary>
    /// Gets the offsets of the buyable patches that the current player can both afford and place somewhere on the board.
    /// </summary>
    /// <returns>A list of buyable patch offsets, a subset of {0, 1, 2}.</returns>
    public List<int> GetBuyablePatchOffsets()
    {
        var currentGame = rootService.CurrentGame
            ?? throw new InvalidOperationException("There is no current game.");

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

    public void BuyPatch(int patchOffset)
    {
        var currentGame = rootService.CurrentGame
            ?? throw new InvalidOperationException("There is no current game.");

        var currentPlayer = currentGame.CurrentPlayer;
        var buyablePatchOffsets = GetBuyablePatchOffsets();

        if (!buyablePatchOffsets.Contains(patchOffset))
        {
            throw new InvalidOperationException("The selected patch cannot be bought by the current player.");
        }

        // Removes the path from the patch shop.
        var patch = currentGame.PatchShop.TakePatch(patchOffset);
        // The player pay the price of the patch.
        currentPlayer.Money -= patch.MoneyCost;
    }
}
