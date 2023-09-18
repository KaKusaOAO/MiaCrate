using MiaCrate.Client.Systems;
using MiaCrate.Resources;
using MiaCrate.World.Inventories;

namespace MiaCrate.Client.Graphics;

public class TextureAtlas : AbstractTexture, IDumpable, ITickable
{
    public static readonly ResourceLocation LocationBlocks = InventoryMenu.BlockAtlas;
    public static readonly ResourceLocation LocationParticles = new("textures/atlas/particles.png");
    
    public ResourceLocation Location { get; }
    public int MaxSupportedTextureSize { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public TextureAtlas(ResourceLocation location)
    {
        Location = location;
        MaxSupportedTextureSize = RenderSystem.MaxSupportedTextureSize;
    }
    
    public override void Load(IResourceManager manager)
    {
        
    }

    public void DumpContents(ResourceLocation location, string path)
    {
        throw new NotImplementedException();
    }

    public void Tick()
    {
        throw new NotImplementedException();
    }

    public void ClearTextureData()
    {
        
    }
}