using MiaCrate.Client.Graphics;
using MiaCrate.Client.Resources;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.UI.Screens;

public class LoadingOverlay : Overlay
{
    private static readonly ResourceLocation _mojangStudiosLogoLocation = new("textures/gui/title/mojangstudios.png");
    
    private readonly Game _game;
    private readonly IReloadInstance _reload;
    private readonly Action<IOptional<Exception>> _onFinish;
    private readonly bool _fadeIn;
    private long _fadeOutStart = -1;
    private long _fadeInStart = -1;

    public LoadingOverlay(Game game, IReloadInstance reload, Action<IOptional<Exception>> onFinish, bool fadeIn)
    {
        _game = game;
        _reload = reload;
        _onFinish = onFinish;
        _fadeIn = fadeIn;
    }

    public static void RegisterTextures(Game game)
    {
        game.TextureManager.Register(_mojangStudiosLogoLocation, new LogoTexture());
    }
    
    public override void Render(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        var m = Util.GetMillis();
        var g = _fadeOutStart > -1 ? (m - _fadeOutStart) / 1000f : -1;
        var h = _fadeInStart > -1 ? (m - _fadeInStart) / 500f : -1;
        
        if (_fadeOutStart == -1 && _reload.IsDone) // && (!_fadeIn || h > 2))
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

    private class LogoTexture : SimpleTexture
    {
        public LogoTexture() : base(_mojangStudiosLogoLocation)
        {
        }

        protected override TextureImage GetTextureImage(IResourceManager manager)
        {
            var resources = Game.Instance.VanillaPackResources;
            var supplier = resources.GetResource(PackType.ClientResources, _mojangStudiosLogoLocation);
            if (supplier == null)
                return new TextureImage(new FileNotFoundException(_mojangStudiosLogoLocation));

            using var stream = supplier();
            return new TextureImage(new TextureMetadataSection(true, true), NativeImage.Read(stream));
        }
    }
}