using MiaCrate.Client.UI;
using MiaCrate.Client.UI.Narration;
using MiaCrate.Client.Utils;
using MiaCrate.Texts;
using Mochi.Texts;
using Mochi.Utils;
using Vortice.DXGI;

namespace MiaCrate.Client.Sodium.UI;

public abstract class AbstractSodiumWidget : IRenderable, IGuiEventListener, INarratableEntry
{
    protected bool _hovered;
    protected bool _focused;
    
    protected Font Font { get; }

    public bool IsFocused
    {
        get => _focused;
        set
        {
            if (!value) _focused = false;
            else
            {
                Util.LogFoobar();
            }
        }
    }

    public bool IsHovered => _hovered;

    public NarrationPriority NarrationPriority
    {
        get
        {
            if (IsFocused)
                return NarrationPriority.Focused;

            if (_hovered)
                return NarrationPriority.Hovered;

            return NarrationPriority.None;
        }
    }

    protected AbstractSodiumWidget()
    {
        Font = Game.Instance.Font;
    }

    public abstract void Render(GuiGraphics graphics, int mouseX, int mouseY, float f);

    public void UpdateNarration(INarrationElementOutput output)
    {
        if (_focused)
            output.Add(NarratedElementType.Usage, MiaComponent.Translatable("narration.button.usage.focused"));
        
        if (_hovered)
            output.Add(NarratedElementType.Usage, MiaComponent.Translatable("narration.button.usage.hovered"));
    }

    protected void DrawString(GuiGraphics graphics, string str, int x, int y, Argb32 color) => 
        graphics.DrawString(Font, str, x, y, color);
    
    protected void DrawString(GuiGraphics graphics, IComponent str, int x, int y, Argb32 color) => 
        graphics.DrawString(Font, str, x, y, color);

    protected void DrawRect(GuiGraphics graphics, int x1, int y1, int x2, int y2, Argb32 color) =>
        graphics.Fill(x1, y1, x2, y2, color);

    protected void DrawBorder(GuiGraphics graphics, int x1, int y1, int x2, int y2, Argb32 color)
    {
        graphics.Fill(x1, y1, x2, y1 + 1, color);
        graphics.Fill(x1, y2 - 1, x2, y2, color);
        graphics.Fill(x1, y1, x1 + 1, y2, color);
        graphics.Fill(x2 - 1, y1, x2, y2, color);
    }

    protected void PlayClickSound()
    {
        Util.LogFoobar();
    }
    
    protected static void RenderScrollingString(GuiGraphics graphics, Font font, IComponent component, int x1, int y1,
        int x2, int y2, Argb32 m) =>
        RenderScrollingString(graphics, font, component, (x1 + x2) / 2, x1, y1, x2, y2, m);

    protected static void RenderScrollingString(GuiGraphics graphics, Font font, IComponent component, int centerX, int x1,
        int y1, int x2, int y2, Argb32 n)
    {
        var o = font.Width(component);
        var p = (y1 + y2 - Font.LineHeight) / 2 + 1;
        var q = x2 - x1;

        int r;
        if (o > q)
        {
            r = o - q;

            var d = Util.GetMillis() / 1000.0;
            var e = Math.Max(r * 0.5, 3.0);
            var f = Math.Sin(Math.PI / 2 * Math.Cos(Math.PI * 2 * d / e)) / 2.0 + 0.5;
            var g = Mth.Lerp(0.0, (double) r, f);

            graphics.EnableScissor(x1, y1, x2, y2);
            graphics.DrawString(font, component, x1 - (int) g, p, n);
            graphics.DisableScissor();
        }
        else
        {
            r = Math.Clamp(centerX, x1 + o / 2, x2 - o / 2);
            graphics.DrawCenteredString(font, component, r, p, n);
        }
    }

    public virtual bool MouseClicked(double x, double y, MouseButton button) => false;
    public virtual bool KeyPressed(Keys key, int scancode, KeyModifiers modifiers) => false;
    public virtual bool MouseDragged(double x, double y, MouseButton button, double dx, double dy) => false;
}