using MiaCrate.Server.Levels;

namespace MiaCrate.World;

public delegate void ChunkStatusUpdateListener(ChunkPos pos, FullChunkStatus status);

public static class ChunkStatusUpdateExtension
{
    public static void OnChunkStatusChange(this ChunkStatusUpdateListener self, ChunkPos pos, FullChunkStatus status) =>
        self(pos, status);
}