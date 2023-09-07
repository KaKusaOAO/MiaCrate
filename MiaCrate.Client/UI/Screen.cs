using Mochi.Texts;

namespace MiaCrate.Client.UI;

public class Screen : AbstractContainerEventHandler
{
    public override List<IGuiEventListener> Children { get; } = new();
    public IComponent Title { get; }

    protected Screen(IComponent title)
    {
        Title = title;
    }
    
    public virtual void Tick() {}
    public virtual void Removed() {}
    public virtual void Added() {}

    public virtual void RenderBackground(GuiGraphics graphics)
    {
        
    }

    public virtual bool IsPauseScreen => true;
}