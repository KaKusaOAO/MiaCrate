using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Resources;
using MiaCrate.Client.Systems;
using MiaCrate.Client.Utils;
using MiaCrate.Resources;
using Mochi.Utils;
using Veldrid;

namespace MiaCrate.Client.UI.Screens;

public class LoadingOverlay : Overlay
{
    private static ResourceLocation MojangStudiosLogoLocation { get; } = new("textures/gui/title/mojangstudios.png");

    // @formatter:off
    private static Argb32 LogoBackgroundColor     { get; } = new(239, 50, 61);
    private static Argb32 LogoBackgroundColorDark { get; } = new(0, 0 ,0);
    private static Func<Argb32> BrandBackground   { get; } = () => LogoBackgroundColor;
    // @formatter:on

    public override bool IsPauseScreen => true;

    private readonly Game _game;
    private readonly IReloadInstance _reload;
    private readonly Action<IOptional<Exception>> _onFinish;
    private readonly bool _fadeIn;
    private long _fadeOutStart = -1;
    private long _fadeInStart = -1;
    private float _currentProgress;

    public LoadingOverlay(Game game, IReloadInstance reload, Action<IOptional<Exception>> onFinish, bool fadeIn)
    {
        _game = game;
        _reload = reload;
        _onFinish = onFinish;
        _fadeIn = fadeIn;
    }

    public static void RegisterTextures(Game game)
    {
        game.TextureManager.Register(MojangStudiosLogoLocation, new LogoTexture());
    }

    public override void Render(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        var cl = GlStateManager.CommandList;
        var width = graphics.GuiWidth;
        var height = graphics.GuiHeight;
        var millis = Util.GetMillis();

        if (_fadeIn && _fadeInStart == -1)
        {
            _fadeInStart = millis;
        }

        var fadeOutProgress = _fadeOutStart > -1 ? (millis - _fadeOutStart) / 1000f : -1;
        var fadeInProgress = _fadeInStart > -1 ? (millis - _fadeInStart) / 500f : -1;
        float logoAlpha;

        if (fadeOutProgress >= 1)
        {
            _game.Screen?.Render(graphics, 0, 0, f);
            
            var alpha = (byte) Math.Ceiling((1 - Math.Clamp(fadeOutProgress - 1, 0, 1)) * 255);
            graphics.Fill(RenderType.GuiOverlay, 0, 0, width, height, BrandBackground().WithAlpha(alpha));
            logoAlpha = 1 - Math.Clamp(fadeOutProgress - 1, 0, 1);
        } 
        else if (_fadeIn)
        {
            if (fadeInProgress < 1) _game.Screen?.Render(graphics, mouseX, mouseY, f);
            
            var alpha = (byte) Math.Ceiling(Math.Clamp(fadeInProgress, 0.15, 1) * 255);
            graphics.Fill(RenderType.GuiOverlay, 0, 0, width, height, BrandBackground().WithAlpha(alpha));
            logoAlpha = Math.Clamp(fadeInProgress, 0, 1);
        }
        else
        {
            var color = BrandBackground();
            cl.ClearColorTarget(0, new RgbaFloat(
                color.Red / 255f, color.Green / 255f, color.Blue / 255f, 1));
            logoAlpha = 1;
        }

        var centerX = (int) (graphics.GuiWidth * 0.5f);
        var centerY = (int) (graphics.GuiHeight * 0.5f);
        var d = Math.Min(graphics.GuiWidth * 0.75, graphics.GuiHeight) * 0.25;
        var t = (int) (d * 0.5);
        var e = d * 4;
        var u = (int) (e * 0.5);
        
        RenderSystem.DisableDepthTest();
        {
            RenderSystem.DepthMask(false);
            {
                RenderSystem.EnableBlend();
                {
                    RenderSystem.BlendFunc(BlendFactor.SourceAlpha, BlendFactor.One);
                    {
                        graphics.SetColor(1, 1, 1, logoAlpha);
                        graphics.Blit(MojangStudiosLogoLocation, centerX - u, centerY - t, u, (int) d, -0.0625f, 0f, 120, 60, 120, 120);
                        graphics.Blit(MojangStudiosLogoLocation, centerX, centerY - t, u, (int) d, 0.0625f, 60f, 120, 60, 120, 120);

                        graphics.SetColor(1, 1, 1, 1);
                    }
                    RenderSystem.DefaultBlendFunc();
                }
                RenderSystem.DisableBlend();
            }
            RenderSystem.DepthMask(true);
        }
        RenderSystem.EnableDepthTest();

        var v = (int) (graphics.GuiHeight * 0.8325);
        var w = _reload.ActualProgress;
        _currentProgress = Math.Clamp(_currentProgress * 0.95f + w * 0.050000012f, 0, 1);

        if (fadeOutProgress < 1)
        {
            DrawProgressBar(graphics, width / 2 - u, v - 5, width / 2 + u, v + 5, 1 - Math.Clamp(fadeOutProgress, 0, 1));
        }

        if (fadeOutProgress >= 2)
        {
            _game.Overlay = null;
        }

        // Start fading out if reload is done
        if (_fadeOutStart == -1 && _reload.IsDone && (!_fadeIn || fadeInProgress >= 2))
        {
            try
            {
                _reload.CheckExceptions();
                _onFinish(Optional.Empty<Exception>());
            }
            catch (Exception ex)
            {
                _onFinish(Optional.Of(ex));
            }

            _fadeOutStart = Util.GetMillis();
            _game.Screen?.Init(_game, graphics.GuiWidth, graphics.GuiHeight);
        }
    }

    private void DrawProgressBar(GuiGraphics graphics, int x, int y, int width, int height, float alpha)
    {
        var m = (int) Math.Ceiling((width - x - 2) * _currentProgress);
        var n = (byte) Math.Round(alpha * byte.MaxValue);
        var color = new Argb32(255, 255, 255, n);
        
        // The inner rect of the progress bar
        graphics.Fill(x + 2, y + 2, x + m, height - 2, color);
        
        // The outer border of the progress bar
        graphics.Fill(x + 1, y, width - 1, y + 1, color);
        graphics.Fill(x + 1, height, width - 1, height - 1, color);
        graphics.Fill(x, y, x + 1, height, color);
        graphics.Fill(width, y, width - 1, height, color);
    }

    private class LogoTexture : SimpleTexture
    {
        public LogoTexture() : base(MojangStudiosLogoLocation)
        {
        }

        protected override TextureImage GetTextureImage(IResourceManager manager)
        {
            var resources = Game.Instance.VanillaPackResources;
            var supplier = resources.GetResource(PackType.ClientResources, MojangStudiosLogoLocation);
            if (supplier == null)
                return new TextureImage(new FileNotFoundException(MojangStudiosLogoLocation));

            using var stream = supplier();
            return new TextureImage(new TextureMetadataSection(true, true), NativeImage.Read(stream));
        }
    }
}