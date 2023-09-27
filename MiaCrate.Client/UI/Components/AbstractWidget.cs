using MiaCrate.Client.Platform;
using MiaCrate.Client.Resources;
using MiaCrate.Client.Sounds;
using MiaCrate.Client.UI.Narration;
using MiaCrate.Sounds;
using MiaCrate.Texts;
using Mochi.Texts;
using Mochi.Utils;
using OpenTK.Windowing.GraphicsLibraryFramework;

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

    protected void RenderScrollingString(GuiGraphics graphics, Font font, int i, int j)
    {
        var k = X + i;
        var l = X + Width - i;
        RenderScrollingString(graphics, font, Message, k, Y, l, Y + Height, j);
    }

    protected static void RenderScrollingString(GuiGraphics graphics, Font font, IComponent component, int i, int j,
        int k, int l, int m) =>
        RenderScrollingString(graphics, font, component, (i + k) / 2, i, j, k, l, m);

    protected static void RenderScrollingString(GuiGraphics graphics, Font font, IComponent component, int i, int j,
        int k, int l, int m, int n)
    {
        var o = font.Width(component);
        var p = (k + m - 9) / 2 + 1;
        var q = l - j;

        int r;
        if (o > q)
        {
            r = o - q;

            var d = Util.GetMillis() / 1000.0;
            var e = Math.Max(r * 0.5, 3.0);
            var f = Math.Sin(1.5707963267948966 * Math.Cos(6.283185307179586 * d / e)) / 2.0 + 0.5;
            var g = Mth.Lerp(f, 0.0, (double) r);

            graphics.EnableScissor(j, k, l, m);
            graphics.DrawString(font, component, j - (int) g, p, n);
            graphics.DisableScissor();
        }
        else
        {
            r = Math.Clamp(i, j + o / 2, l - o / 2);
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
    protected virtual void OnDrag(double x1, double y1, double x2, double y2) {}

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

    public virtual void PlayDownSound(SoundManager manager)
    {
        manager.Play(SimpleSoundInstance.ForUi(SoundEvents.UiButtonClick, 1f));
    }

    protected virtual bool IsValidClickButton(MouseButton button) => button == MouseButton.Left;
}