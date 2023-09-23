using MiaCrate.Client.Graphics;
using MiaCrate.Client.Systems;
using Mochi.Texts;

namespace MiaCrate.Client.UI.Screens;

public class TitleScreen : Screen
{
    private const string DemoLevelId = "Demo_World";
    public static readonly CubeMap CubeMap = new(new ResourceLocation("textures/gui/title/background/panorama"));
    
    private readonly bool _fading;
    private readonly LogoRenderer _logoRenderer;
    private readonly PanoramaRenderer _panorama;
    private long _fadeInStart;

    private static readonly ResourceLocation _panoramaOverlay =
        new("textures/gui/title/background/panorama_overlay.png");


    public TitleScreen(bool fading = false, LogoRenderer? logoRenderer = null) : base(TranslateText.Of("narrator.screen.title"))
    {
        _panorama = new PanoramaRenderer(CubeMap);
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

    public override void RenderBackground(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        
    }

    public override void Render(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        if (_fadeInStart == 0 && _fading)
        {
            _fadeInStart = Util.GetMillis();
        }

        var g = _fading ? (Util.GetMillis() - _fadeInStart) / 1000f : 1f;
        _panorama.Render(f, Math.Clamp(g - 1, 0, 1));
        
        RenderSystem.EnableBlend();
        graphics.SetColor(1, 1, 1, _fading ? MathF.Ceiling(Math.Clamp(g, 0, 1)) : 1);
        graphics.Blit(_panoramaOverlay, 0, 0, Width, Height, 0, 0, 16, 128, 16, 128);
        graphics.SetColor(1, 1, 1, 1);

        var h = _fading ? Math.Clamp(g - 1, 0, 1) : 1;
        _logoRenderer.RenderLogo(graphics, Width, h);
    }
}