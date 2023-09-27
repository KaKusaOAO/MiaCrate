using MiaCrate.Client.UI.Narration;
using Mochi.Texts;

namespace MiaCrate.Client.UI.Screens;

public class Screen : AbstractContainerEventHandler, IRenderable
{
    public static ResourceLocation BackgroundLocation { get; } = new("textures/gui/options_background.png");
    
    private readonly List<IRenderable> _renderables = new();
    private readonly List<INarratableEntry> _narratables = new();
    private bool _initialized;

    public override List<IGuiEventListener> Children { get; } = new();
    public IComponent Title { get; }

    protected Font Font { get; private set; }
    protected Game? Game { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    protected Screen(IComponent title)
    {
        Title = title;
    }
    
    public virtual void Tick() {}
    public virtual void Removed() {}
    public virtual void Added() {}

    public virtual void RenderBackground(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        if (Game!.Level != null)
        {
            RenderTransparentBackground(graphics);
        }
        else
        {
            RenderDirtBackground(graphics);
        }
    }

    public void RenderTransparentBackground(GuiGraphics graphics)
    {
        graphics.FillGradient(0, 0, Width, Height, 0xc0101010, 0xd0101010);
    }
    
    public virtual void RenderDirtBackground(GuiGraphics graphics)
    {
        graphics.SetColor(0.25f, 0.25f, 0.25f, 1);
        graphics.Blit(BackgroundLocation, 0, 0, 0, 0f, 0f, Width, Height, 32, 32);
        graphics.SetColor(1, 1, 1, 1);
    }

    public virtual bool IsPauseScreen => true;

    public void Init(Game game, int width, int height)
    {
        Game = game;
        Font = game.Font;
        
        Width = width;
        Height = height;

        if (!_initialized)
        {
            Init();
        }
        else
        {
            RepositionElements();
        }

        _initialized = true;
    }

    public virtual void Render(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        RenderBackground(graphics, mouseX, mouseY, f);

        foreach (var renderable in _renderables)
        {
            renderable.Render(graphics, mouseX, mouseY, f);
        }
    }

    public void RenderWithTooltip(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        Render(graphics, mouseX, mouseY, f);
    }

    public void Resize(Game game, int width, int height)
    {
        Width = width;
        Height = height;
        RepositionElements();
    }
    
    protected virtual void RepositionElements() => RebuildWidgets();

    protected void RebuildWidgets()
    {
        ClearWidgets();
        ClearFocus();
        Init();
    }

    private void ClearFocus()
    {
        
    }

    protected void ClearWidgets()
    {
        _renderables.Clear();
        Children.Clear();
    }

    protected virtual void Init()
    {
        
    }

    protected virtual T AddRenderableWidget<T>(T widget) where T : IGuiEventListener, IRenderable, INarratableEntry
    {
        _renderables.Add(widget);
        return AddWidget(widget);
    }

    protected virtual T AddRenderable<T>(T widget) where T : IRenderable
    {
        _renderables.Add(widget);
        return widget;
    }
    
    protected virtual T AddWidget<T>(T widget) where T : IGuiEventListener, INarratableEntry
    {
        Children.Add(widget);
        _narratables.Add(widget);
        return widget;
    }

    protected void RemoveWidget(IGuiEventListener widget)
    {
        if (widget is IRenderable r) 
            _renderables.Remove(r);
        
        if (widget is INarratableEntry n) 
            _narratables.Remove(n);
        
        Children.Remove(widget);
    }
}