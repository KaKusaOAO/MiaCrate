using MiaCrate.Client.Graphics;
using Mochi.Texts;

namespace MiaCrate.Client.UI;

public class TitleScreen : Screen
{
    private const string DemoLevelId = "Demo_World";
    public static readonly CubeMap CubeMap = new(new ResourceLocation("textures/gui/title/background/panorama"));
    
    private readonly bool _fading;
    private readonly LogoRenderer _logoRenderer;
    private readonly PanoramaRenderer _panorama;

    private static readonly ResourceLocation _panoramaOverlay =
        new("textures/gui/title/background/panorama_overlay.png");

    public TitleScreen(bool fading = false, LogoRenderer? logoRenderer = null) : base(TranslateText.Of("narrator.screen.title"))
    {
        _panorama = new PanoramaRenderer();
        _fading = fading;
        _logoRenderer = logoRenderer ?? new LogoRenderer(false);
    }

    public override bool IsPauseScreen => false;

    public static Task PreloadResourcesAsync(TextureManager manager, IExecutor executor)
    {
        return Task.WhenAll(
            manager.PreloadAsync(LogoRenderer.Logo, executor),
            manager.PreloadAsync(LogoRenderer.Edition, executor),
            manager.PreloadAsync(_panoramaOverlay, executor),
            CubeMap.PreloadAsync(manager, executor)
        );
    }
}