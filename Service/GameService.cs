namespace Patchwork.Service;

/// <summary>
/// Provides game-level operations.
/// </summary>
public class GameService(RootService rootService)
{
    private readonly RootService _rootService = rootService;
}
