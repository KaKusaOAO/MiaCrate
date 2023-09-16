using Mochi.Texts;

namespace MiaCrate.Client.UI;

public abstract class AbstractButton : AbstractWidget
{
    protected AbstractButton(int x, int y, int width, int height, IComponent message)
        : base(x, y, width, height, message)
    {
        
    }

    public abstract void OnPress();

    protected override void RenderWidget(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        throw new NotImplementedException();
    }
}