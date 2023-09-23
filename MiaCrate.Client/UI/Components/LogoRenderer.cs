namespace MiaCrate.Client.UI;

public class LogoRenderer
{
    public static ResourceLocation Logo { get; } = new("textures/gui/title/minecraft.png");
    public static ResourceLocation EasterEggLogo { get; } = new("textures/gui/title/minceraft.png");
    public static ResourceLocation Edition { get; } = new("textures/gui/title/edition.png");

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

    public void RenderLogo(GuiGraphics graphics, int screenWidth, float alpha, int y = DefaultHeightOffset)
    {
        graphics.SetColor(1, 1, 1, _keepLogoThroughFade ? 1 : alpha);
        
        var logoX = screenWidth / 2 - LogoWidth / 2;
        graphics.Blit(_showEasterEgg ? EasterEggLogo : Logo, logoX, y, 0, 0, 
            LogoWidth, LogoHeight, LogoTextureWidth, LogoTextureHeight);

        var editionX = screenWidth / 2 - EditionWidth / 2;
        var editionY = y + LogoHeight - EditionLogoOverlap;
        graphics.Blit(Edition, editionX, editionY, 0, 0, 
        EditionWidth, EditionHeight, EditionTextureWidth, EditionTextureHeight);
        graphics.SetColor(1, 1, 1, 1);
    }
}