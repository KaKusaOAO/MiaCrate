using Mochi.Texts;

namespace MiaCrate.Client.UI;

public class TitleScreen : Screen
{
    private readonly bool _fading;
    private readonly LogoRenderer _logoRenderer;
    private readonly PanoramaRenderer _panorama;

    public TitleScreen(bool fading = false, LogoRenderer? logoRenderer = null) : base(TranslateText.Of("narrator.screen.title"))
    {
        _panorama = new PanoramaRenderer();
        _fading = fading;
        _logoRenderer = logoRenderer ?? new LogoRenderer(false);
    }

    public override bool IsPauseScreen => false;
}