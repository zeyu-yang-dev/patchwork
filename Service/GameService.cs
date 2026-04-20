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
    }
    
    
    
}
