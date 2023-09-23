using Mochi.Texts;

namespace MiaCrate.Client.UI.Screens;

public class Screen : AbstractContainerEventHandler, IRenderable
{
    private readonly List<IRenderable> _renderables = new();
    
    public override List<IGuiEventListener> Children { get; } = new();
    public IComponent Title { get; }

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
        
    }

    public virtual bool IsPauseScreen => true;

    public void Init(Game game, int width, int height)
    {
        Game = game;
        Width = width;
        Height = height;
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
}