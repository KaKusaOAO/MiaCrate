using MiaCrate.Client.Models;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class BlockEntityWithoutLevelRenderer : IResourceManagerReloadListener
{
    public BlockEntityWithoutLevelRenderer(BlockEntityRenderDispatcher blockEntityRenderDispatcher,
        EntityModelSet models)
    {
        
    }

    public void OnResourceManagerReload(IResourceManager manager)
    {
        Util.LogFoobar();
    }
}