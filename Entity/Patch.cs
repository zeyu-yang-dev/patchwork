using Patchwork.Data;

namespace Patchwork.Entity;

public class Patch(int id)
{
    public int Id { get; } = id;

    public bool[,] Shape { get; } = PatchData.Data[id].shape;

    public int MoneyCost { get; } = PatchData.Data[id].moneyCost;

    public int TimeCost { get; } = PatchData.Data[id].timeCost;

    public int Income { get; } = PatchData.Data[id].income;
}