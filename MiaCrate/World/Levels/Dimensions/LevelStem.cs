using MiaCrate.Core;

namespace MiaCrate.World.Dimensions;

public record LevelStem(IHolder<DimensionType> Type, ChunkGenerator Generator);