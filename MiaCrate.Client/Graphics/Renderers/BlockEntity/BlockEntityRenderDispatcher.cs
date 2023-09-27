using MiaCrate.Client.Models;
using MiaCrate.Client.UI;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class BlockEntityRenderDispatcher : IResourceManagerReloadListener
{
    public BlockEntityRenderDispatcher(Font font, EntityModelSet entityModelSet, Func<BlockRenderDispatcher> supplier,
        Func<ItemRenderer> supplier2, Func<EntityRenderDispatcher> supplier3)
    {
        
    }

    public void OnResourceManagerReload(IResourceManager manager)
    {
        Util.LogFoobar();
    }
}