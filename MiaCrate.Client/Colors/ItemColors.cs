using MiaCrate.World.Items;

namespace MiaCrate.Client.Colors;

public class ItemColors
{
    public static ItemColors CreateDefault()
    {
        Util.LogFoobar();
        return new ItemColors();
    }
}

public delegate int ItemColorDelegate(ItemStack itemStack, int i);