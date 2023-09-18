using MiaCrate.Client.Models;
using MiaCrate.Resources;

namespace MiaCrate.Client.Graphics;

public class BlockEntityWithoutLevelRenderer : IResourceManagerReloadListener
{
    public BlockEntityWithoutLevelRenderer(BlockEntityRenderDispatcher blockEntityRenderDispatcher,
        EntityModelSet models)
    {
        
    }

    public void OnResourceManagerReload(IResourceManager manager)
    {
        throw new NotImplementedException();
    }
}