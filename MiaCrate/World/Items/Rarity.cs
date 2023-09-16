using Mochi.Texts;

namespace MiaCrate.World.Items;

public sealed class Rarity
{
    public static readonly Rarity Common = new(TextColor.White);
    public static readonly Rarity Uncommon = new(TextColor.Yellow);
    public static readonly Rarity Rare = new(TextColor.Aqua);
    public static readonly Rarity Epic = new(TextColor.Purple);
    
    public TextColor Color { get; }

    private Rarity(TextColor color)
    {
        Color = color;
    }
}