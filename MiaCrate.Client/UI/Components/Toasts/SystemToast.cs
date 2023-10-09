namespace MiaCrate.Client.UI.Toasts;

public class SystemToast : IToast<SystemToastIds>
{
    private const int MaxLineSize = 200;
    private const int LineSpacing = 12;
    private const int Margin = 10;

    public SystemToastIds Token => throw new NotImplementedException();

    public ToastVisibility Render(GuiGraphics graphics, ToastComponent toast, long l)
    {
        throw new NotImplementedException();
    }
}

public sealed class SystemToastIds
{
    public static SystemToastIds TutorialHint { get; } = new();
    public static SystemToastIds NarratorToggle { get; } = new();
    public static SystemToastIds UnsecureServerWarning { get; } = new(10000);

    private readonly long _displayTime;

    private SystemToastIds(long displayTime = 5000)
    {
        _displayTime = displayTime;
    }
}