namespace Patchwork.Entity;

public class Patch(int id, bool[,] shape, int moneyCost, int timeCost, int income)
{
    public int Id { get; } = id;

    public bool[,] Shape { get; } = shape;

    public int MoneyCost { get; } = moneyCost;

    public int TimeCost { get; } = timeCost;

    public int Income { get; } = income;
}