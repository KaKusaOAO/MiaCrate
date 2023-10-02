using MiaCrate.Client.Graphics;
using MiaCrate.Client.Systems;
using MiaCrate.Client.Utils;
using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate.Client.UI.Screens;

public class TitleScreen : Screen
{
    private const string DemoLevelId = "Demo_World";

    public static IComponent CopyrightText { get; } = MiaComponent.Translatable("title.credits");
    public static CubeMap CubeMap { get; } = new(new ResourceLocation("textures/gui/title/background/panorama"));
    private static ResourceLocation PanoramaOverlay { get; } = new("textures/gui/title/background/panorama_overlay.png");
    
    private readonly bool _fading;
    private readonly LogoRenderer _logoRenderer;
    private readonly PanoramaRenderer _panorama;
    private SplashRenderer? _splash;
    private long _fadeInStart;

    public override bool IsPauseScreen => false;

    public TitleScreen(bool fading = false, LogoRenderer? logoRenderer = null) 
        : base(MiaComponent.Translatable("narrator.screen.title"))
    {
        _panorama = new PanoramaRenderer(CubeMap);
        _fading = fading;
        _logoRenderer = logoRenderer ?? new LogoRenderer(false);
    }

    public static Task PreloadResourcesAsync(TextureManager manager, IExecutor executor)
    {
        return Task.WhenAll(
            manager.PreloadAsync(LogoRenderer.Logo, executor),
            manager.PreloadAsync(LogoRenderer.Edition, executor),
            manager.PreloadAsync(PanoramaOverlay, executor),
            CubeMap.PreloadAsync(manager, executor)
        );
    }

    protected override void Init()
    {
        _splash ??= new SplashRenderer("Now on .NET!");
        
        var i = Font.Width(CopyrightText);
        var j = Width - i - 2;
        var l = Height / 4 + 48;
        CreateNormalMenuOptions(l, 24);

        AddRenderableWidget(
            Button.CreateBuilder(
                    MiaComponent.Translatable("menu.options"),
                    _ => Game!.Screen = new TitleScreen())
                .Bounds(Width / 2 - 100, l + 72 + 12, 98, Button.DefaultHeight)
                .Build()
        );
        
        AddRenderableWidget(
            Button.CreateBuilder(
                    MiaComponent.Translatable("menu.quit"),
                    _ => Game!.Screen = new TitleScreen())
                .Bounds(Width / 2 + 2, l + 72 + 12, 98, Button.DefaultHeight)
                .Build()
        );
        
        AddRenderableWidget(
            new PlainTextButton(j, Height - 10, i, 10, CopyrightText, 
                _ => Game!.Screen = new TitleScreen(), Font)
        );
    }

    private void CreateNormalMenuOptions(int i, int j)
    {
        AddRenderableWidget(
            Button.CreateBuilder(
                MiaComponent.Translatable("menu.singleplayer"),
                _ => Game!.Screen = new TitleScreen())
                .Bounds(Width / 2 - 100, i, 200, Button.DefaultHeight)
                .Build()
        );
        
        AddRenderableWidget(
            Button.CreateBuilder(
                    MiaComponent.Translatable("menu.multiplayer"),
                    _ => Game!.Screen = new TitleScreen())
                .Bounds(Width / 2 - 100, i + j * 1, 200, Button.DefaultHeight)
                .Build()
        );
        
        AddRenderableWidget(
            Button.CreateBuilder(
                    MiaComponent.Translatable("menu.online"),
                    _ => Game!.Screen = new TitleScreen())
                .Bounds(Width / 2 - 100, i + j * 2, 200, Button.DefaultHeight)
                .Build()
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

        var fadeInProgress = _fading ? (Util.GetMillis() - _fadeInStart) / 1000f : 1f;
        _panorama.Render(f, Math.Clamp(fadeInProgress, 0, 1));
        
        RenderSystem.EnableBlend();
        graphics.SetColor(1, 1, 1, _fading ? MathF.Ceiling(Math.Clamp(fadeInProgress, 0, 1)) : 1);
        graphics.Blit(PanoramaOverlay, 0, 0, Width, Height, 0, 0, 16, 128, 16, 128);
        graphics.SetColor(1, 1, 1, 1);

        var h = _fading ? Math.Clamp(fadeInProgress - 1, 0, 1) : 1;
        _logoRenderer.RenderLogo(graphics, Width, h);

        var k = (byte) Math.Ceiling(h * 255);
        if ((k & 0xfc) != 0)
        {
            _splash?.Render(graphics, Width, Font, k);
            
            var str = $"{MiaCore.ProductName} {SharedConstants.CurrentVersion.Name}";
            graphics.DrawString(Font, str, 2, Height - 10, Argb32.White.WithAlpha(k));
            
            foreach (var listener in Children)
            {
                if (listener is AbstractWidget widget)
                {
                    widget.SetAlpha(h);
                }
            }
            
            base.Render(graphics, mouseX, mouseY, f);
        }
    }
}