using Patchwork.Domain;

namespace Patchwork.Service;

/// <summary>
/// Acts as the root entry point of the service layer.
/// </summary>
public class RootService
{
    public GameState CurrentGame { get; private set; }

    public GameService GameService { get; }

    public PlayerActionService PlayerActionService { get; }

    public RootService()
    {
        GameService = new GameService(this);
        PlayerActionService = new PlayerActionService(this);
    }
}
