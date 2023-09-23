using System.Text;
using MiaCrate.Client.Resources;
using MiaCrate.World.Blocks;

namespace MiaCrate.Client.Graphics;

public class BlockModelShaper
{
    public BlockModelShaper(ModelManager modelManager)
    {
        
    }

    public static ModelResourceLocation StateToModelLocation(ResourceLocation location, BlockState state) =>
        new(location, StatePropertiesToString(state.Values));

    public static string StatePropertiesToString(Dictionary<IProperty, IComparable> dict)
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in dict)
        {
            if (sb.Length != 0) sb.Append(',');

            sb.Append(key.Name);
            sb.Append('=');
            sb.Append(GetValue(key, value));
        }

        return sb.ToString();
    }

    private static string GetValue(IProperty property, IComparable comparable) => property.GetName(comparable);
}