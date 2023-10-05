using MiaCrate.World;

namespace MiaCrate.Server.Levels;

public interface IChunkPlayerProvider
{
    public List<ServerPlayer> GetPlayers(ChunkPos pos, bool bl);
}