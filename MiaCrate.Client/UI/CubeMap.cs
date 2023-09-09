using MiaCrate.Client.Graphics;

namespace MiaCrate.Client.UI;

public class CubeMap
{
    private const int Sides = 6;
    
    private readonly ResourceLocation[] _images = new ResourceLocation[Sides];

    public CubeMap(ResourceLocation location)
    {
        for (var i = 0; i < Sides; i++)
        {
            _images[i] = location.WithSuffix($"_{i}.png");
        }
    }

    public void Render(Game game, float f, float g, float h)
    {
        
    } 
    
    public Task PreloadAsync(TextureManager manager, IExecutor executor)
    {
        return Task.WhenAll(
            _images.Select(x => manager.PreloadAsync(x, executor))
        );
    }
}