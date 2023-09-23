namespace MiaCrate.Client.UI;

public enum ScreenAxis
{
    Horizontal,
    Vertical
}

public static class ScreenAxisExtension
{
    public static ScreenAxis Orthogonal(this ScreenAxis axis)
    {
        return axis switch
        {
            ScreenAxis.Horizontal => ScreenAxis.Vertical,
            ScreenAxis.Vertical => ScreenAxis.Horizontal,
            _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
        };
    }
    
    public static ScreenDirection GetPositive(this ScreenAxis axis)
    {
        return axis switch
        {
            ScreenAxis.Horizontal => ScreenDirection.Right,
            ScreenAxis.Vertical => ScreenDirection.Down,
            _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
        };
    }
    
    public static ScreenDirection GetNegative(this ScreenAxis axis)
    {
        return axis switch
        {
            ScreenAxis.Horizontal => ScreenDirection.Left,
            ScreenAxis.Vertical => ScreenDirection.Up,
            _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
        };
    }
    
    public static ScreenDirection GetDirection(this ScreenAxis axis, bool positive) => 
        positive ? axis.GetPositive() : axis.GetNegative();
}