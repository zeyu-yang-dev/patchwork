namespace Patchwork.Domain;

/// <summary>
/// Represents a player in the game.
/// </summary>
public class Player(string name)
{
    public string Name { get; } = name;

    public int Money { get; set; } = 5;
    
    // TODO
    public int TimePosition { get; set; } = 51;

    public PatchBoard PatchBoard { get; } = new();

    public bool HasSevenBySevenBonus { get; set; } = false;

    public int Income => PatchBoard.Income;
    
    // plus 157 to ensure that the score starts from 0
    public int Score => Money - 2 * PatchBoard.CountEmptyCells() + (HasSevenBySevenBonus ? 7 : 0) + 157 + 5;
}
