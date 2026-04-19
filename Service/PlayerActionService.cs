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
    /// Gets the offsets of the buyable patches that the current player can both afford and place somewhere on the board.
    /// </summary>
    /// <returns>A list of buyable patch offsets.</returns>
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

        var patch = currentGame.PatchShop.TakePatch(patchOffset);
        currentPlayer.Money -= patch.MoneyCost;
        
        // TODO: WALK LOGIC

        

    }
}
