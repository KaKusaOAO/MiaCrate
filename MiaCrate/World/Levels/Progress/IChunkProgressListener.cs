namespace MiaCrate.World;

public interface IChunkProgressListener
{
    public void UpdateSpawnPos(ChunkPos pos);
    public void OnStatusChange(ChunkPos pos, ChunkStatus? status);
    public void Start();
    public void Stop();
}