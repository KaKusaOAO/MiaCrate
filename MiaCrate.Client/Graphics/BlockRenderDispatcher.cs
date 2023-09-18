using MiaCrate.Client.Colors;
using MiaCrate.Resources;

namespace MiaCrate.Client.Graphics;

public class BlockRenderDispatcher : IResourceManagerReloadListener
{
    public BlockRenderDispatcher(BlockModelShaper blockModelShaper,
        BlockEntityWithoutLevelRenderer blockEntityWithoutLevelRenderer, BlockColors blockColors)
    {
    }

    public void OnResourceManagerReload(IResourceManager manager)
    {
        throw new NotImplementedException();
    }
}