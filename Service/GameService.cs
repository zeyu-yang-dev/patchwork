using System;
using Patchwork.Domain;

namespace Patchwork.Service;

/// <summary>
/// Provides game-level operations.
/// </summary>
public class GameService(RootService rootService)
{
    public void StartNewGame(string firstPlayerName, string secondPlayerName)
    {
        rootService.CurrentGame = new GameState(firstPlayerName, secondPlayerName);
        
        rootService.NotifyGameStarted();
        rootService.NotifyStateChanged();
    }

    public void EndTurn()
    {
        var currentGame = rootService.CurrentGame;

        // If there is a patch waiting to be placed, the turn shouldn't be ended.
        if (currentGame.CurrentPlacedPatch != null) return;
        
        // Notifies root service when the game should be ended.
        if (currentGame.IsGameOver())
        {
            rootService.NotifyGameEnded();
            return;
        }
        
        // Gets the current player and the other player.
        var currentPlayer = currentGame.CurrentPlayer;
        var currentPlayerIndex = currentGame.CurrentPlayerIndex;
        var otherPlayerIndex = currentPlayerIndex == 0 ? 1 : 0;
        var otherPlayer = currentGame.Players[otherPlayerIndex];
        
        // Decides the next player in turn and switches the current player.
        currentGame.CurrentPlayerIndex = 
            currentPlayer.TimePosition > otherPlayer.TimePosition 
                ? otherPlayerIndex 
                : currentPlayerIndex;
        
        // Notify UI to refresh
        rootService.NotifyStateChanged();
    }
    
    
    
}
