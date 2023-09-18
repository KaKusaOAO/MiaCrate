using MiaCrate.Client.Systems;
using MiaCrate.Resources;

namespace MiaCrate.Client.Graphics;

public class TextureAtlas : AbstractTexture, IDumpable, ITickable
{
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