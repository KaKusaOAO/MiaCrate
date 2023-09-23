namespace MiaCrate.Client.UI;

public enum ScreenDirection
{
    Up,
    Down,
    Left,
    Right
}

public static class ScreenDirectionExtension
{
    public static ScreenAxis GetAxis(this ScreenDirection dir)
    {
        return dir switch
        {
            ScreenDirection.Up or ScreenDirection.Down => ScreenAxis.Vertical,
            ScreenDirection.Left or ScreenDirection.Right => ScreenAxis.Horizontal,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }
    
    public static ScreenDirection GetOpposite(this ScreenDirection dir)
    {
        return dir switch
        {
            ScreenDirection.Up => ScreenDirection.Down,
            ScreenDirection.Down => ScreenDirection.Up,
            ScreenDirection.Left => ScreenDirection.Right,
            ScreenDirection.Right => ScreenDirection.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }

    public static bool IsPositive(this ScreenDirection dir)
    {
        return dir switch
        {
            ScreenDirection.Up or ScreenDirection.Left => false,
            ScreenDirection.Down or ScreenDirection.Right => true,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }

    public static bool IsAfter(this ScreenDirection dir, int i, int j)
    {
        if (dir.IsPositive()) return i > j;
        return j > i;
    }
    
    public static bool IsBefore(this ScreenDirection dir, int i, int j)
    {
        if (dir.IsPositive()) return i < j;
        return j < i;
    }
}