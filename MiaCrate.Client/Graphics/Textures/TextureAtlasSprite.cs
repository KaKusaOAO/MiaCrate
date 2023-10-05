namespace MiaCrate.Client.Graphics;

public class TextureAtlasSprite
{
    public ResourceLocation AtlasLocation { get; }
    public SpriteContents Contents { get; }
    public int X { get; }
    public int Y { get; }
    public float U0 { get; }
    public float U1 { get; }
    public float V0 { get; }
    public float V1 { get; }

    private float AtlasSize
    {
        get
        {
            var f = Contents.Width / (U1 - U0);
            var g = Contents.Height / (V1 - V0);
            return Math.Max(g, f);
        }
    }

    public float UvShrinkRatio => 4 / AtlasSize;

    public TextureAtlasSprite(ResourceLocation atlasLocation, SpriteContents contents, int width, int height, int x, int y)
    {
        AtlasLocation = atlasLocation;
        Contents = contents;
        X = x;
        Y = y;
        U0 = (float) x / width;
        U1 = (float) (x + contents.Width) / width;
        V0 = (float) y / height;
        V1 = (float) (y + contents.Height) / height;
    }

    public float GetU(float f)
    {
        // Lerp?
        var g = U1 - U0;
        return U0 + g * f;
    }
    
    public float GetV(float f)
    {
        // Lerp?
        var g = V1 - V0;
        return V0 + g * f;
    }

    public ITicker? CreateTicker()
    {
        var spriteTicker = Contents.CreateTicker();
        return spriteTicker != null ? new TickerInstance(this, spriteTicker) : null;
    }

    public void UploadFirstFrame()
    {
        Contents.UploadFirstFrame(X, Y);
    }

    private class TickerInstance : ITicker
    {
        private readonly TextureAtlasSprite _sprite;
        private readonly ISpriteTicker _ticker;

        public TickerInstance(TextureAtlasSprite sprite, ISpriteTicker ticker)
        {
            _sprite = sprite;
            _ticker = ticker;
        }

        public void TickAndUpload() => _ticker.TickAndUpload(_sprite.X, _sprite.Y);

        public void Dispose() => _ticker.Dispose();
    }
    
    public interface ITicker : IDisposable
    {
        public void TickAndUpload();
    }
}