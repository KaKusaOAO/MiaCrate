namespace MiaCrate.Client.UI;

public abstract class AbstractWidget : IRenderable, IGuiEventListener, ILayoutElement, INarratableEntry
{
    public static readonly ResourceLocation WidgetsLocation = new("textures/gui/widgets.png");
    
    public void Render(GuiGraphics graphics, int i, int j, float f)
    {
        throw new NotImplementedException();
    }

    public virtual bool IsFocused { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public int Width => throw new NotImplementedException();

    public int Height => throw new NotImplementedException();

    public NarrationPriority NarrationPriority => throw new NotImplementedException();
}