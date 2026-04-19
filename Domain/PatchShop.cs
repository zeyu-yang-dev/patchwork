using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Patchwork.Domain;

/// <summary>
/// Represents the patch shop of the game.
/// </summary>
public class PatchShop
{
    // 33 patches in total, id from 0 to 32.
    private const int PatchCount = 33;
    
    private const int SmallestPatchId = 0;

    public List<Patch> Patches { get; }
    
    // Index of the first patch among the three selectable patches in clockwise order.
    public int NeutralTokenIndex { get; set; }

    public PatchShop()
    {
        Patches = CreateShuffledPatches();
        NeutralTokenIndex = FindInitialNeutralTokenIndex(Patches);
    }

    /// <summary>
    /// Gets the patch at the given clockwise offset starting from <see cref="NeutralTokenIndex"/>.
    /// </summary>
    /// <param name="offset">The clockwise offset from the first selectable patch.</param>
    /// <returns>The patch at the specified offset.</returns>
    public Patch GetPatchByOffset(int offset)
    {
        if (Patches.Count == 0)
        {
            throw new InvalidOperationException("The patch shop is empty.");
        }

        if (offset is < 0 or > 2)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be 0, 1, or 2.");
        }

        var index = (NeutralTokenIndex + offset) % Patches.Count;
        return Patches[index];
    }

    /// <summary>
    /// Gets the first three selectable patches in clockwise order.
    /// </summary>
    /// <returns>A list containing the currently selectable patches.</returns>
    public List<Patch> GetSelectablePatches()
    {
        return GetPatchesInRange(0, Math.Min(3, Patches.Count));
    }

    /// <summary>
    /// Gets all patches that come after the selectable patches in clockwise order.
    /// </summary>
    /// <returns>A list containing all non-selectable patches after the first three selectable patches.</returns>
    public List<Patch> GetPatchesAfterSelectable()
    {
        if (Patches.Count <= 3)
        {
            return [];
        }

        return GetPatchesInRange(3, Patches.Count - 3);
    }

    private static List<Patch> CreateShuffledPatches()
    {
        var patches = new List<Patch>();

        for (var id = 0; id < PatchCount; id++)
        {
            patches.Add(new Patch(id));
        }

        Random.Shared.Shuffle(CollectionsMarshal.AsSpan(patches));

        return patches;
    }

    private static int FindInitialNeutralTokenIndex(List<Patch> patches)
    {
        var smallestPatchIndex = patches.FindIndex(smallestPatch => smallestPatch.Id == SmallestPatchId);
        return (smallestPatchIndex + 1) % patches.Count;
    }

    /// <summary>
    /// Gets a list of patches in clockwise order starting from the given offset relative to <see cref="NeutralTokenIndex"/>.
    /// </summary>
    /// <param name="startOffset">The clockwise offset of the first patch to include.</param>
    /// <param name="count">The number of patches to include.</param>
    /// <returns>A list of patches in clockwise order.</returns>
    private List<Patch> GetPatchesInRange(int startOffset, int count)
    {
        var patches = new List<Patch>(count);

        for (var offset = startOffset; offset < startOffset + count; offset++)
        {
            patches.Add(GetPatchByOffset(offset));
        }

        return patches;
    }
}
