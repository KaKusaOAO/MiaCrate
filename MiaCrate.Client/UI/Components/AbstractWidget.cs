using MiaCrate.Client.Platform;
using MiaCrate.Client.Sounds;
using MiaCrate.Client.UI.Narration;
using Mochi.Texts;
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
        
    }

    protected virtual bool IsValidClickButton(MouseButton button) => button == MouseButton.Left;
}