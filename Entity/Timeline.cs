using System.Collections.Generic;

namespace Patchwork.Entity;

/// <summary>
/// Represents the timeline of the game.
/// </summary>
public class Timeline()
{
    public int StartPosition { get; } = 0;

    public int EndPosition { get; } = 53;

    // when a player reaches the following positions,
    // they receive income
    public IReadOnlyList<int> IncomePositions { get; } =
    [
        5, 11, 17, 23, 29, 35, 41, 47, 53
    ];

    public List<int> RemainingSpecialPatchPositions { get; } =
    [
        20, 26, 32, 44, 50
    ];
}