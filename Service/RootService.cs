using System;
using System.Collections.Generic;
using Patchwork.Domain;

namespace Patchwork.Service;

/// <summary>
/// Acts as the root entry point of the service layer.
/// </summary>
public class RootService
{
    public event Action StateChanged;
    public event Action GameStarted;
    
    public event Action<int, int> AdvanceStarted;
    public event Action<List<int>, int, int> IncomeChecked;
    public event Action<int?> SpecialPatchChecked;
    
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

    public void NotifyAdvanceStarted(int startPosition, int targetPosition)
    {
        AdvanceStarted?.Invoke(startPosition, targetPosition);
    }

    public void NotifyIncomeChecked(List<int> incomePositions, int startPosition, int targetPosition)
    {
        IncomeChecked?.Invoke(incomePositions,  startPosition, targetPosition);
    }

    public void NotifySpecialPatchChecked(int? indexInFullArray)
    {
        SpecialPatchChecked?.Invoke(indexInFullArray);
    }
}
