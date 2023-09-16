using MiaCrate.Client.Models;
using MiaCrate.Resources;

namespace MiaCrate.Client.Graphics;

public class BlockEntityRenderDispatcher : IResourceManagerReloadListener
{
    public BlockEntityRenderDispatcher(Font font, EntityModelSet entityModelSet, Func<BlockRenderDispatcher> supplier,
        Func<ItemRenderer> supplier2, Func<EntityRenderDispatcher> supplier3)
    {
        
    }
}