using MiaCrate.Client.Platform;
using MiaCrate.Client.Resources;
using MiaCrate.Client.Sounds;
using MiaCrate.Client.UI.Narration;
using MiaCrate.Client.Utils;
using MiaCrate.Sounds;
using MiaCrate.Texts;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.Client.UI;

public abstract class AbstractWidget : IRenderable, IGuiEventListener, ILayoutElement, INarratableEntry
{
    public static readonly ResourceLocation WidgetsLocation = new("textures/gui/widgets.png");

    public bool IsHovered { get; private set; }
    public virtual bool IsFocused { get; set; }
    public virtual bool IsHoveredOrFocused => IsHovered || IsFocused;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public IComponent Message { get; set; }
    protected float Alpha { get; set; } = 1f;
    public bool IsVisible { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public virtual bool IsNarrativeActive => IsVisible && IsActive;
    public Tooltip? Tooltip { get; set; }

    public NarrationPriority NarrationPriority
    {
        get
        {
            if (IsFocused) return NarrationPriority.Focused;
            return IsHovered
                ? NarrationPriority.Hovered
                : NarrationPriority.None;
        }
    }

    bool INarratableEntry.IsActive => IsNarrativeActive;

    protected AbstractWidget(int x, int y, int width, int height, IComponent message)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Message = message;
    }

    public void SetAlpha(float alpha) => Alpha = alpha;

    public void Render(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        if (!IsVisible) return;

        IsHovered = mouseX >= X && mouseY >= Y && mouseX < X + Width && mouseY < Y + Height;
        RenderWidget(graphics, mouseX, mouseY, f);
        UpdateTooltip();
    }

    protected abstract void RenderWidget(GuiGraphics graphics, int mouseX, int mouseY, float f);

    protected void RenderScrollingString(GuiGraphics graphics, Font font, int margin, Argb32 color)
    {
        var x1 = X + margin;
        var x2 = X + Width - margin;
        RenderScrollingString(graphics, font, Message, x1, Y, x2, Y + Height, color);
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

    private void UpdateTooltip()
    {
        if (Tooltip == null) return;
    }

    protected virtual IMutableComponent CreateNarrationMessage() =>
        WrapDefaultNarrationMessage(Message);

    public static IMutableComponent WrapDefaultNarrationMessage(IComponent component) => 
        TranslateText.Of("gui.narrate.button", component);

    public void UpdateNarration(INarrationElementOutput output)
    {
        UpdateWidgetNarration(output);
        Tooltip?.UpdateNarration(output);
    }

    protected abstract void UpdateWidgetNarration(INarrationElementOutput output);

    public virtual void OnClick(double x, double y) {}
    public virtual void OnRelease(double x, double y) {}
    protected virtual void OnDrag(double x, double y, double dx, double dy) {}

    protected bool Clicked(double x, double y) =>
        IsActive && IsVisible && x >= X && y >= Y && x < X + Width && y < Y + Height;
    
    public virtual bool MouseClicked(double x, double y, MouseButton button)
    {
        if (!IsActive || !IsVisible) return false;
        if (!IsValidClickButton(button)) return false;
        if (!Clicked(x, y)) return false;
        
        PlayDownSound(Game.Instance.SoundManager);
        OnClick(x, y);
        return true;
    }

    public virtual bool MouseDragged(double x, double y, MouseButton button, double dx, double dy)
    {
        if (!IsValidClickButton(button)) return false;
        OnDrag(x, y, dx, dy);
        return true;
    }

    public virtual void PlayDownSound(SoundManager manager)
    {
        manager.Play(SimpleSoundInstance.ForUi(SoundEvents.UiButtonClick, 1f));
    }

    protected virtual bool IsValidClickButton(MouseButton button) => button == MouseButton.Left;
    
    public void VisitWidgets(Action<AbstractWidget> consumer) => consumer(this);
}