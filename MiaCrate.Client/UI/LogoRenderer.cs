namespace MiaCrate.Client.UI;

public class LogoRenderer
{
    public static readonly ResourceLocation Logo = new("textures/gui/title/minecraft.png");
    public static readonly ResourceLocation EasterEggLogo = new("textures/gui/title/minceraft.png");
    public static readonly ResourceLocation Edition = new("textures/gui/title/edition.png");

    public const int LogoWidth = 256;
    public const int LogoHeight = 44;
    private const int LogoTextureWidth = 256;
    private const int LogoTextureHeight = 64;
    private const int EditionWidth = 128;
    private const int EditionHeight = 14;
    private const int EditionTextureWidth = 128;
    private const int EditionTextureHeight = 16;
    public const int DefaultHeightOffset = 30;
    private const int EditionLogoOverlap = 7;

    private readonly bool _showEasterEgg = IRandomSource.Create().NextSingle() < 1.0e-4; 
    private readonly bool _keepLogoThroughFade;

    public LogoRenderer(bool keepLogoThroughFade)
    {
        _keepLogoThroughFade = keepLogoThroughFade;
    }

    public void RenderLogo(GuiGraphics graphics, int i, float f, int j = DefaultHeightOffset)
    {
        
    }
}