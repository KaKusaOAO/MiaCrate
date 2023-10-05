using MiaCrate.Core;

namespace MiaCrate.World;

public class BiomeManager
{
    
}

public interface INoiseBiomeSource
{
    public IHolder<Biome> GetNoiseBiome(int x, int y, int z);
}