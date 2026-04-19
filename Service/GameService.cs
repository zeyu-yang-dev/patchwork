using Patchwork.Domain;

namespace Patchwork.Service;

/// <summary>
/// Provides game-level operations.
/// </summary>
public class GameService(RootService rootService)
{
    private readonly RootService _rootService = rootService;

    public void StartNewGame(string firstPlayerName, string secondPlayerName)
    {
        _rootService.CurrentGame = new GameState(firstPlayerName, secondPlayerName);
    }
    
    
    
}
