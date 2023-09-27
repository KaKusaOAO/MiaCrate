using MiaCrate.Client.Systems;
using MiaCrate.Client.Utils;
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
        graphics.SetColor(1, 1, 1, 1);

        var k = IsActive ? 0xFFFFFF : 0xA0A0A0;
        RenderString(graphics, game.Font, ((Argb32) k).WithAlpha((byte) Math.Ceiling(Alpha * 255)));
    }

    public void RenderString(GuiGraphics graphics, Font font, int i) => 
        RenderScrollingString(graphics, font, 2, i);
}