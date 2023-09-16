namespace MiaCrate.Client.UI;

public abstract class Overlay : IRenderable
{
    public bool IsPauseScreen => true;
    public abstract void Render(GuiGraphics graphics, int mouseX, int mouseY, float f);
}