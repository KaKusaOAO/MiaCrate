namespace MiaCrate.Client.Graphics;

public class StitcherException : Exception
{
    public IEnumerable<IStitcherEntry> AllSprites { get; }

    public StitcherException(IStitcherEntry entry, IEnumerable<IStitcherEntry> sprites)
        : base(
            $"Unable to fit: {entry.Name} - size: {entry.Width}x{entry.Height} - Maybe try a lower resolution resourcepack?")
    {
        AllSprites = sprites;
    }
}