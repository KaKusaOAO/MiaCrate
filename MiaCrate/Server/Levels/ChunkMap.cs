using MiaCrate.World;

namespace MiaCrate.Server.Levels;

public class ChunkMap : ChunkStorage, IChunkPlayerProvider
{
    private const int ChunkSavedPerTick = 200;
    private const int ChunkSavedEagerlyPerTick = 20;
    private const int EagerChunkSaveCooldownInMillis = 10000;

    public const int MinViewDistance = 1 << 1;
    public const int MaxViewDistance = 1 << 5;
    
    public List<ServerPlayer> GetPlayers(ChunkPos pos, bool bl)
    {
        throw new NotImplementedException();
    }
}