using MiaCrate.Client.Models;
using MiaCrate.Client.Multiplayer;
using MiaCrate.Client.UI;
using MiaCrate.Resources;
using MiaCrate.World;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class EntityRenderDispatcher : IResourceManagerReloadListener
{
    private const float MaxShadowRadius = 32f;
    private const float ShadowPowerFalloffY = 0.5f;

    private Level? _level;
    public Camera? Camera { get; set; }
    
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

    public void SetLevel(ClientLevel? level)
    {
        _level = level;

        if (level == null)
            Camera = null;
    }
}