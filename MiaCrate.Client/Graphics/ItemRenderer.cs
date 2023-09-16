using MiaCrate.Client.Colors;
using MiaCrate.Client.Resources;
using MiaCrate.Resources;

namespace MiaCrate.Client.Graphics;

public class ItemRenderer : IResourceManagerReloadListener
{
    public ItemModelShaper ItemModelShaper { get; }
    
    public ItemRenderer(Game game, TextureManager textureManager, ModelManager modelManager, ItemColors itemColors,
        BlockEntityWithoutLevelRenderer blockEntityRenderer)
    {
        ItemModelShaper = new ItemModelShaper(modelManager);
    }

    public void OnResourceManagerReload(IResourceManager manager)
    {
        ItemModelShaper.RebuildCache();
    }
}