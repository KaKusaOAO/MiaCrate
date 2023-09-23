using MiaCrate.Client.Models;
using MiaCrate.Client.Resources;

namespace MiaCrate.Client.Graphics;

public class ItemOverrides
{
    public static ItemOverrides Empty { get; } = new();
    
    private ItemOverrides() {}
    
    public ItemOverrides(IModelBaker modelBaker, BlockModel model, List<ItemOverride> overrides)
    {
        
    }
}