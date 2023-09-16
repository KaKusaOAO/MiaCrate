using MiaCrate.Client.Models;
using MiaCrate.Resources;

namespace MiaCrate.Client.Graphics;

public record EntityRendererContext(EntityRenderDispatcher EntityRenderDispatcher, ItemRenderer ItemRenderer,
    BlockRenderDispatcher BlockRenderDispatcher, ItemInHandRenderer ItemInHandRenderer,
    IResourceManager ResourceManager, EntityModelSet ModelSet, Font Font)
{
    public ModelPart BakeLayer(ModelLayerLocation location) => ModelSet.BakeLayer(location);
}