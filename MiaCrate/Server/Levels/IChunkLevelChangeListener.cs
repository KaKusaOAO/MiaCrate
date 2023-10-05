using MiaCrate.World;

namespace MiaCrate.Server.Levels;

public interface IChunkLevelChangeListener
{
    public void OnLevelChange(ChunkPos pos, Func<int> sup, int i, Action<int> consumer);
}