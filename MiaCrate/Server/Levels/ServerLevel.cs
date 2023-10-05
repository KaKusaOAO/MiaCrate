using MiaCrate.Common;
using MiaCrate.Core;
using MiaCrate.Resources;
using MiaCrate.World;
using MiaCrate.World.Blocks;
using MiaCrate.World.Dimensions;
using MiaCrate.World.Entities;

namespace MiaCrate.Server.Levels;

public class ServerLevel : Level, IWorldGenLevel
{
    public static BlockPos EndSpawnPoint { get; } = new(100, 50, 0);
    public static IntProvider RainDelay { get; } = UniformInt.Of(12000, 180000);
    public static IntProvider RainDuration { get; } = UniformInt.Of(12000, 24000);
    public static IntProvider ThunderDelay { get; } = UniformInt.Of(12000, 180000);
    public static IntProvider ThunderDuration { get; } = UniformInt.Of(3600, 15600);

    private const int EmptyTimeNoTick = 300;
    private const int MaxScheduledTicksPerTick = 0x10000;

    public List<ServerPlayer> ServerPlayers { get; } = new();

    public ServerChunkCache ServerChunkSource { get; }
    public override ChunkSource ChunkSource => ServerChunkSource;

    public override List<Player> Players => ServerPlayers.Cast<Player>().ToList();
    
    public long Seed => throw new NotImplementedException();

    public ServerLevel(GameServer server, IExecutor executor, LevelStorageAccess levelStorageAccess, 
        ServerLevelData levelData, IResourceKey<Level> dimension, LevelStem stem, IChunkProgressListener listener,
        bool bl, long l, List<ICustomSpawner> spawners, bool bl2, RandomSequences? randomSequences) 
        : base(levelData, dimension, server.RegistryAccess, stem.Type, () => server.Profiler, false, 
            bl, l, server.MaxChainedNeighborUpdates)
    {
        // ServerChunkSource = new ServerChunkCache(this, levelStorageAccess, )
    }

    public override void OnBlockStateChange(BlockPos pos, BlockState oldState, BlockState newState)
    {
        Util.LogFoobar();
    }

    public void SetCurrentlyGenerating(Func<string>? currentlyGenerating)
    {
        throw new NotImplementedException();
    }

    public override bool AddFreshEntity(Entity entity) => AddEntity(entity);

    private bool AddEntity(Entity entity)
    {
        throw new NotImplementedException();
    }

    public void UpdateSleepingPlayerList()
    {
        throw new NotImplementedException();
    }

    private sealed class EntityCallbacks : ILevelCallback<Entity>
    {
        private ServerLevel _instance;

        public EntityCallbacks(ServerLevel instance)
        {
            _instance = instance;
        }

        public void OnTrackingStart(Entity entity)
        {
            // _instance.ChunkSource
            
            if (entity is ServerPlayer sp)
            {
                _instance.ServerPlayers.Add(sp);
                _instance.UpdateSleepingPlayerList();
            }

            if (entity is Mob mob)
            {
                Util.LogFoobar();
            }
        }
    }
}