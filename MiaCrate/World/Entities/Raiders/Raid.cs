using MiaCrate.Core;
using MiaCrate.Server.Levels;
using MiaCrate.Texts;
using Mochi.Nbt;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.World.Entities;

public class Raid
{
    private const int SectionRadiusForFindingNewVillageCenter = 2;
    private const int VillageSearchRadius = 32;
    private const int RaidTimeoutTicks = 40 * SharedConstants.TicksPerMinute;
    private const int NumSpawnAttempts = 3;
    private const string OminousBannerPatternName = "block.minecraft.ominous_banner";
    private const string RaidersRemaining = "event.minecraft.raid.raiders_remaining";
    public const int VillageRadiusBuffer = 16;
    private const int PostRaidTickLimit = 2 * SharedConstants.TicksPerSecond;
    private const int DefaultPreRaidTicks = 15 * SharedConstants.TicksPerSecond;
    public const int MaxNoActionTime = 2 * SharedConstants.TicksPerMinute;
    public const int MaxCelebrationTicks = 30 * SharedConstants.TicksPerSecond;
    private const int OutsideRaidBoundsTimeout = 30;
    public const int TicksPerDay = SharedConstants.TicksPerGameDay;
    public const int DefaultMaxBadOmenLevel = 5;
    private const int LowMobThreshold = 2;
    private const int HeroOfTheVillageDuration = 2 * SharedConstants.TicksPerGameDay;
    public const int ValidRaidRadiusSqr = 9216;
    public const int RaidRemovalThresholdSqr = 12544;

    private static IComponent RaidNameComponent { get; } = MiaComponent.Translatable("event.minecraft.raid");
    private static IComponent RaidBarVictoryComponent { get; } = MiaComponent.Translatable("event.minecraft.raid.victory.full");
    private static IComponent RaidBarDefeatComponent { get; } = MiaComponent.Translatable("event.minecraft.raid.defeat.full");

    private readonly Dictionary<int, Raider> _groupToLeaderMap = new();
    private readonly Dictionary<int, HashSet<Raider>> _groupRaiderMap = new();
    private readonly HashSet<Uuid> _heroesOfTheVillage = new();

    private readonly int _id;
    private BlockPos _center;
    private long _ticksActive;
    private bool _started;
    private int _badOmenLevel;
    private int _groupSpawned;
    private int _postRaidTicks;
    private int _raidCooldownTicks;
    private readonly int _numGroups;
    private int _celebrationTicks;
    private readonly IRandomSource _random = IRandomSource.Create();
    private IOptional<BlockPos> _waveSpawnPos = Optional.Empty<BlockPos>();
        
    public ServerLevel Level { get; }
    public float TotalHealth { get; private set; }
    public bool IsActive { get; private set; }

    public Raid(int id, ServerLevel level, BlockPos center)
    {
        _id = id;
        Level = level;
        _center = center;
        _raidCooldownTicks = 15 * SharedConstants.TicksPerSecond;
        
        throw new NotImplementedException();
    }

    public Raid(ServerLevel level, NbtCompound tag)
    {
        throw new NotImplementedException();
    }
}