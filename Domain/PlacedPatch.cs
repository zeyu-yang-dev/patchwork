namespace Patchwork.Domain;

/// <summary>
/// Represents a patch in its current placement state.
/// </summary>
/// <param name="patch">The patch definition.</param>
public class PlacedPatch(Patch patch)
{
    public Patch Patch { get; } = patch;

    // (col, row) corresponds to (x, y) in patch board
    public (int col, int row)? Coordinate { get; set; } = null;

    // Rotation = 0/90/180/270
    public int Rotation { get; set; } = 0;

    // Whether the original shape needs to be mirrored to get the shape of this placed patch
    public bool IsMirrored { get; set; } = false;

    
    /// Returns the current shape of this placed patch.
    /// The base shape is first mirrored if needed, and then rotated.
    public bool[,] GetCurrentShape()
    {
        var currentShape = Patch.Shape;

        if (IsMirrored)
        {
            currentShape = MirrorShape(currentShape);
        }

        currentShape = RotateShape(currentShape, Rotation);

        return currentShape;
    }
    
    private static bool[,] RotateShape(bool[,] shape, int rotation)
    {
        var rotatedShape = shape;

        switch (rotation)
        {
            case 0:
                break;

            case 90:
                rotatedShape = RotateShape90(rotatedShape);
                break;

            case 180:
                rotatedShape = RotateShape90(RotateShape90(rotatedShape));
                break;

            case 270:
                rotatedShape = RotateShape90(RotateShape90(RotateShape90(rotatedShape)));
                break;
        }

        return rotatedShape;
    }

    public static bool[,] RotateShape90(bool[,] shape)
    {
        var rotatedShape = new bool[5, 5];

        for (var row = 0; row < 5; row++)
        {
            for (var col = 0; col < 5; col++)
            {
                // clockwise 90 degrees
                rotatedShape[row, col] = shape[4 - col, row];
            }
        }

        return rotatedShape;
    }
    
    public static bool[,] MirrorShape(bool[,] shape)
    {
        var mirroredShape = new bool[5, 5];

        for (var row = 0; row < 5; row++)
        {
            for (var col = 0; col < 5; col++)
            {
                mirroredShape[row, col] = shape[row, 4 - col];
            }
        }

        return mirroredShape;
    }
}
