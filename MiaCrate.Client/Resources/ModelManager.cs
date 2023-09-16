using MiaCrate.Client.Colors;
using MiaCrate.Client.Graphics;
using MiaCrate.Resources;

namespace MiaCrate.Client.Resources;

public class ModelManager : IPreparableReloadListener, IDisposable
{
    public BlockModelShaper BlockModelShaper { get; }
    
    public ModelManager(TextureManager textureManager, BlockColors blockColors, int maxMipmapLevels)
    {
        BlockModelShaper = new BlockModelShaper(this);
    }
}