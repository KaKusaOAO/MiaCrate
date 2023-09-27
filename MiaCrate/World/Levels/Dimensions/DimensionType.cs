using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Tags;
using MiaCrate.World.Blocks;

namespace MiaCrate.World.Dimensions;

public record DimensionType(long? FixedTime, bool HasSkyLight, bool HasCeiling, bool UltraWarm, bool Natural,
    double CoordinateScale, bool BedWorks, bool RespawnAnchorWorks, int MinY, int Height, int LogicalHeight,
    ITagKey<Block> InfiniBurn, ResourceLocation EffectsLocation, float AmbientLight, 
    DimensionType.MonsterSettingRecord MonsterSettings)
{
    public static double GetTeleportationScale(DimensionType from, DimensionType to)
    {
        var d = from.CoordinateScale;
        var e = to.CoordinateScale;
        return d / e;
    }
    
    public record MonsterSettingRecord(bool PiglinSafe, bool HasRaids, IntProvider MonsterSpawnLightTest,
        int MonsterSpawnBlockLightLimit)
    {
        public static IMapCodec<MonsterSettingRecord> Codec { get; } =
            RecordCodecBuilder.MapCodec<MonsterSettingRecord>(instance => instance
                .Group(
                    Data.Codec.Bool.FieldOf("piglin_safe").ForGetter<MonsterSettingRecord>(r => r.PiglinSafe),
                    Data.Codec.Bool.FieldOf("has_raids").ForGetter<MonsterSettingRecord>(r => r.HasRaids),
                    IntProvider.CreateCodec(0, 15).FieldOf("monster_spawn_light_level")
                        .ForGetter<MonsterSettingRecord>(r => r.MonsterSpawnLightTest),
                    Data.Codec.IntRange(0, 15).FieldOf("monster_spawn_block_light_limit")
                        .ForGetter<MonsterSettingRecord>(r => r.MonsterSpawnBlockLightLimit)
                )
                .Apply<MonsterSettingRecord>(instance)
            );
    }
}