using MiaCrate.World;

namespace MiaCrate.Server.Levels;

public class ChunkTaskPriorityQueueSorter : IChunkLevelChangeListener, IDisposable
{
    public void OnLevelChange(ChunkPos pos, Func<int> sup, int i, Action<int> consumer)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public sealed class Message<T>
    {
        
    }
}