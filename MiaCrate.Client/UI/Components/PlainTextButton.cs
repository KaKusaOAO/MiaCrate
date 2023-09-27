using MiaCrate.Client.Utils;
using MiaCrate.Extensions;
using Mochi.Texts;

namespace MiaCrate.Client.UI;

public class PlainTextButton : Button
{
    private readonly Font _font;
    private readonly IComponent _underlinedMessage;

    public PlainTextButton(int x, int y, int width, int height, IComponent component, OnPressDelegate onPress, Font font) 
        : base(x, y, width, height, component, onPress, DefaultNarration)
    {
        _font = font;
        _underlinedMessage = component.Clone().WithUnderlined(true);
    }

    protected override void RenderWidget(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        var component = IsHoveredOrFocused ? _underlinedMessage : Message;
        graphics.DrawString(_font, component, X, Y, Argb32.White.WithAlpha(Alpha));
    }
}