namespace MiaCrate.Client.UI.Screens;

public abstract class Overlay : IRenderable
{
    public virtual bool IsPauseScreen => true;
    public abstract void Render(GuiGraphics graphics, int mouseX, int mouseY, float f);
}