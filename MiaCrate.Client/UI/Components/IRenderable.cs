namespace MiaCrate.Client.UI;

public interface IRenderable
{
    public void Render(GuiGraphics graphics, int mouseX, int mouseY, float f);
}