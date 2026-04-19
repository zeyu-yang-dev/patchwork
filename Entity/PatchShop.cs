using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Patchwork.Entity;

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
}
