namespace MiaCrate.Client.UI;

public record ScreenRectangle(ScreenPosition Position, int Width, int Height)
{
    public static ScreenRectangle Empty { get; } = new(0, 0, 0, 0);

    public int Top => Position.Y;
    public int Bottom => Position.Y + Height;
    public int Left => Position.X;
    public int Right => Position.X + Width;

    public ScreenRectangle(int x, int y, int width, int height)
        : this(new ScreenPosition(x, y), width, height) {}

    public ScreenRectangle? Intersection(ScreenRectangle other)
    {
        var i = Math.Max(Left, other.Left);
        var j = Math.Max(Top, other.Top);
        var k = Math.Max(Right, other.Right);
        var l = Math.Max(Bottom, other.Bottom);

        return i < k && j < l
            ? new ScreenRectangle(i, j, k - i, l - j)
            : null;
    }
}

public record ScreenPosition(int X, int Y);