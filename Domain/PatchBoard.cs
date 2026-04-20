using System;
using System.Collections.Generic;

namespace Patchwork.Domain;

/// <summary>
/// Represents a player's patch board.
/// </summary>
public class PatchBoard
{
    public const int Size = 9;

    // use readonly to define an unchangeable reference
    private readonly List<PlacedPatch> _placedPatches = new();
    // for access from outside
    public IReadOnlyList<PlacedPatch> PlacedPatches => _placedPatches;
    
    private readonly bool[,] _cells = new bool[Size, Size];
    
    public int Income
    {
        get
        {
            var income = 0;

            foreach (var placedPatch in _placedPatches)
            {
                income += placedPatch.Patch.Income;
            }

            return income;
        }
    }

    // =================================================================================================================

    /// <summary>
    /// Checks whether the given patch can be placed anywhere on this board
    /// in any mirrored state and any rotation.
    /// </summary>
    public bool IsPlaceable(Patch patch)
    {

        foreach (var isMirrored in new[] { false, true })
        {
            foreach (var rotation in new[] { 0, 90, 180, 270 })
            {
                var placedPatch = new PlacedPatch(patch)
                {
                    IsMirrored = isMirrored,
                    Rotation = rotation
                };

                for (var row = 0; row < Size; row++)
                {
                    for (var col = 0; col < Size; col++)
                    {
                        if (IsPlaceable(placedPatch, col, row))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether the given patch (with its mirror and rotation)
    /// can be placed on this board
    /// with its center at the given board coordinate.
    /// </summary>
    public bool IsPlaceable(PlacedPatch placedPatch, int col, int row)
    {
        ArgumentNullException.ThrowIfNull(placedPatch);

        var shape = placedPatch.GetCurrentShape();

        for (var shapeRow = 0; shapeRow < 5; shapeRow++)
        {
            for (var shapeCol = 0; shapeCol < 5; shapeCol++)
            {
                // if the cell of the 5*5 matrix is empty, skip
                if (!shape[shapeRow, shapeCol])
                {
                    continue;
                }

                var boardCol = col + shapeCol - 2;
                var boardRow = row + shapeRow - 2;

                // if an unempty cell of the 5*5 matrix exceeds the 9*9 matrix
                if (!IsInsideBoard(boardCol, boardRow))
                {
                    return false;
                }

                // if the non-empty cell of the 5*5 matrix lands on an occupied cell
                if (IsCellOccupied(boardCol, boardRow))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Places the given patch on the board with its center at the given board coordinate.
    /// Throws an exception if the placement is invalid.
    /// </summary>
    public void PlacePatch(PlacedPatch placedPatch, int col, int row)
    {
        ArgumentNullException.ThrowIfNull(placedPatch);

        if (!IsPlaceable(placedPatch, col, row))
        {
            throw new InvalidOperationException("Cannot place the patch here.");
        }

        // 1. Changes the coordinate inside the PlacedPatch.
        placedPatch.Coordinate = (col, row);
        // 2. Adds the new PlacedPatch to this patch board.
        _placedPatches.Add(placedPatch);
        // 3. Fills the cells according to the current shape of the new patch.
        var shape = placedPatch.GetCurrentShape();
        for (var shapeRow = 0; shapeRow < 5; shapeRow++)
        {
            for (var shapeCol = 0; shapeCol < 5; shapeCol++)
            {
                if (!shape[shapeRow, shapeCol])
                {
                    continue;
                }

                var boardCol = col + shapeCol - 2;
                var boardRow = row + shapeRow - 2;

                _cells[boardRow, boardCol] = true;
            }
        }
    }

    // =================================================================================================================
    
    private bool IsCellOccupied(int col, int row)
    {
        if (!IsInsideBoard(col, row))
        {
            throw new ArgumentException("The given coordinate is outside the board.");
        }

        return _cells[row, col];
    }

    /// <summary>
    /// Counts the number of occupied cells on the board.
    /// </summary>
    public int CountOccupiedCells()
    {
        var count = 0;

        for (var row = 0; row < Size; row++)
        {
            for (var col = 0; col < Size; col++)
            {
                if (IsCellOccupied(col, row))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Counts the number of empty cells on the board.
    /// </summary>
    public int CountEmptyCells()
    {
        return Size * Size - CountOccupiedCells();
    }

    /// <summary>
    /// Checks whether the board contains any completely filled 7x7 area.
    /// </summary>
    public bool HasFilled7x7Area()
    {
        for (var startRow = 0; startRow <= Size - 7; startRow++)
        {
            for (var startCol = 0; startCol <= Size - 7; startCol++)
            {
                if (Is7x7AreaFilled(startCol, startRow))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether the specified 7x7 area is completely filled.
    /// </summary>
    /// <param name="startCol">X coordinate of the top-left cell in the 7x7 area</param>
    /// <param name="startRow">Y coordinate of the top-left cell in the 7x7 area</param>
    /// <returns>True if the specified 7x7 area is filled.</returns>
    private bool Is7x7AreaFilled(int startCol, int startRow)
    {
        for (var row = startRow; row < startRow + 7; row++)
        {
            for (var col = startCol; col < startCol + 7; col++)
            {
                if (!IsCellOccupied(col, row))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool IsInsideBoard(int col, int row)
    {
        return col >= 0 && col < Size && row >= 0 && row < Size;
    }
}
