using MiaCrate.Core;

namespace MiaCrate.World;

public abstract class Structure
{
    public record StructureSettings(IHolderSet<Biome> Biomes,
        Dictionary<MobCategory, StructureSpawnOverride> SpawnOverrides, GenerationStep.Decoration Step,
        TerrainAdjustment TerrainAdaption)
    {
        
    }
}

