namespace MiaCrate.Client.UI;

public enum NarrationPriority
{
    None,
    Hovered,
    Focused
}

public static class NarrationPriorityExtension
{
    public static bool IsTerminal(this NarrationPriority priority) => 
        priority == NarrationPriority.Focused;
}