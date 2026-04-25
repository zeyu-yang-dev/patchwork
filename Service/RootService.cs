using System;
using Patchwork.Domain;

namespace Patchwork.Service;

/// <summary>
/// Acts as the root entry point of the service layer.
/// </summary>
public class RootService
{
    // 用来在状态改变后通知UI层刷新
    public event Action StateChanged;
    public event Action GameStarted;
    
    public GameState CurrentGame { get; set; }

    public GameService GameService { get; }
    public PlayerActionService PlayerActionService { get; }
    public PatchService PatchService { get; }

    public RootService()
    {
        GameService = new GameService(this);
        PlayerActionService = new PlayerActionService(this);
        PatchService = new PatchService(this);
    }
    
    public void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    public void NotifyGameStarted()
    {
        GameStarted?.Invoke();
    }
}
