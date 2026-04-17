using System.Collections.Generic;
using Patchwork.Entity;

namespace Patchwork.Data;

public static class PatchData
{
    public static List<Patch> CreateAllPatches()
    {
        return
        [
            new Patch(
                id: 0,
                shape: new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false, false, false, false },
                    { false, false, true,  true,  false },
                    { false, false, false, false, false },
                    { false, false, false, false, false }
                },
                moneyCost: 0,
                timeCost: 0,
                income: 0
            ),

            new Patch(
                id: 1,
                shape: new bool[,]
                {
                    { false, false, false, false, false },
                    { false, false, true,  false, false },
                    { false, false, true,  true,  true  },
                    { false, false, true,  false, false },
                    { false, false, false, false, false }
                },
                moneyCost: 0,
                timeCost: 0,
                income: 0
            )
        ];
    }
}