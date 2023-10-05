using MiaCrate.Common;
using MiaCrate.World;

namespace MiaCrate.Server.Levels;

public class ThreadedLevelLightEngine : LevelLightEngine, IDisposable
{
    private readonly ChunkMap _chunkMap;
    private readonly ProcessorMailbox<IRunnable> _taskMailbox;
    private readonly IProcessorHandle<ChunkTaskPriorityQueueSorter.Message<IRunnable>> _sorterMailbox;

    public ThreadedLevelLightEngine(ILightChunkGetter level, ChunkMap chunkMap, bool bl,
        ProcessorMailbox<IRunnable> taskMailbox,
        IProcessorHandle<ChunkTaskPriorityQueueSorter.Message<IRunnable>> sorterMailbox) 
        : base(level, true, bl)
    {
        _chunkMap = chunkMap;
        _taskMailbox = taskMailbox;
        _sorterMailbox = sorterMailbox;
    }
    
    public void Dispose()
    {
    }
}