using MiaCrate.Client.Colors;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class BlockRenderDispatcher : IResourceManagerReloadListener
{
    public BlockRenderDispatcher(BlockModelShaper blockModelShaper,
        BlockEntityWithoutLevelRenderer blockEntityWithoutLevelRenderer, BlockColors blockColors)
    {
    }

    public void OnResourceManagerReload(IResourceManager manager)
    {
        Util.LogFoobar();
    }
}