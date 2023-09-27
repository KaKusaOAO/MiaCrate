namespace MiaCrate.Client.Fonts;

public interface ISheetGlyphInfo
{
    public const float DefaultBearingY = 3;

    public int PixelWidth { get; }
    public int PixelHeight { get; }
    public bool IsColored { get; }
    public float Oversample { get; }

    public float BearingX => 0;
    public float BearingY => DefaultBearingY;
    public float Left => BearingX;
    public float Right => Left + PixelWidth / Oversample;
    public float Up => BearingY;
    public float Down => Up + PixelHeight / Oversample;

    public void Upload(int x, int y);
}