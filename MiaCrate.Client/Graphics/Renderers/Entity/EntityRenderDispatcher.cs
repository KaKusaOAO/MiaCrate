using MiaCrate.Client.Models;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class EntityRenderDispatcher : IResourceManagerReloadListener
{
    private const float MaxShadowRadius = 32f;
    private const float ShadowPowerFalloffY = 0.5f;
    
    public ItemInHandRenderer ItemInHandRenderer { get; }

    public EntityRenderDispatcher(Game game, TextureManager textureManager, ItemRenderer itemRenderer,
        BlockRenderDispatcher blockRenderDispatcher, Font font, Options options, EntityModelSet models)
    {
        ItemInHandRenderer = new ItemInHandRenderer(game, this, itemRenderer);
    }

    public void OnResourceManagerReload(IResourceManager manager)
    {
        Util.LogFoobar();
    }
}