using MiaCrate.Server.Levels;

namespace MiaCrate.World;

public interface ILevelCallback<in T>
{
    public void OnTrackingStart(T obj);
}

public delegate void ChunkStatusUpdateListener(ChunkPos pos, FullChunkStatus status);