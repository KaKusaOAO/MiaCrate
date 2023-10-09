namespace MiaCrate.Client.UI.Toasts;

public interface IToast
{
    public const int SlotHeight = 32;

    public static object NoToken { get; } = new();
    
    public object Token => NoToken;
    public int Width => 160;
    public int Height => SlotHeight;
    public int SlotCount => Util.PositiveCeilDiv(Height, SlotHeight);

    public ToastVisibility Render(GuiGraphics graphics, ToastComponent toast, long l);
}

public interface IToast<out T> : IToast
{
    public new T Token { get; }
    object IToast.Token => Token!;
}