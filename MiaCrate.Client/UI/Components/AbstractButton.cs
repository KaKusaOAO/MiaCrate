using MiaCrate.Client.Systems;
using Mochi.Texts;

namespace MiaCrate.Client.UI;

public abstract class AbstractButton : AbstractWidget
{
    protected const int TextMargin = 2;

    private static WidgetSprites Sprites { get; } = new(
        new ResourceLocation("widget/button"),
        new ResourceLocation("widget/button_disabled"),
        new ResourceLocation("widget/button_highlighted")
    );
    
    protected AbstractButton(int x, int y, int width, int height, IComponent message)
        : base(x, y, width, height, message)
    {
        
    }

    public abstract void OnPress();

    protected override void RenderWidget(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        var game = Game.Instance;
        graphics.SetColor(1, 1, 1, Alpha);
        RenderSystem.EnableBlend();
        RenderSystem.EnableDepthTest();

        graphics.BlitSprite(Sprites.Get(IsActive, IsHoveredOrFocused), X, Y, Width, Height);
    }
}