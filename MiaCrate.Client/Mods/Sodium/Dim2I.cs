namespace MiaCrate.Client.Sodium;

public record Dim2I(int X, int Y, int Width, int Height)
{
    public int LimitX => X + Width;
    public int LimitY => Y + Height;
    public int CenterX => X + Width / 2;
    public int CenterY => Y + Height / 2;

    public bool ContainsCursor(double x, double y) =>
        x >= X && x < LimitX && y >= Y && y < LimitY;
}