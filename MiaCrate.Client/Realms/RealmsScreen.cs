using MiaCrate.Client.Systems;
using MiaCrate.Client.UI;
using MiaCrate.Client.UI.Screens;
using MiaCrate.Resources;
using MiaCrate.Texts;
using Mochi.Texts;
using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Realms;

public abstract class RealmsScreen : Screen
{
    protected const int TitleHeight = 17;
    protected const int ExpirationNotificationDays = 7;

    protected RealmsScreen(IComponent title) : base(title)
    {
    }
}

public class RealmsPopupScreen : RealmsScreen
{
    public static readonly IComponent PopupText = MiaComponent.Translatable("mco.selectServer.popup");

    private static List<ResourceLocation> _carouselImages = new();
    private readonly Screen _backgroundScreen;
    private readonly bool _trialAvailable;

    public RealmsPopupScreen(Screen backgroundScreen, bool trialAvailable) : base(PopupText)
    {
        _backgroundScreen = backgroundScreen;
        _trialAvailable = trialAvailable;
    }

    public static void UpdateCarouselImages(IResourceManager manager)
    {
        var resources = manager
            .ListResources("textures/gui/images", l => l.Path.EndsWith(".png"))
            .Keys;

        _carouselImages = resources
            .Where(l => l.Namespace == ResourceLocation.RealmsNamespace)
            .ToList();
    }

    public override void RenderBackground(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        _backgroundScreen.Render(graphics, -1, -1, f);
        graphics.Flush();
        RenderSystem.Clear(ClearBufferMask.DepthBufferBit, Game.OnMacOs);
    }
}