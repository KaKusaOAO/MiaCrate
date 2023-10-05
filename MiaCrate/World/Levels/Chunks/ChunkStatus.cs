using MiaCrate.Common;
using MiaCrate.Core;
using MiaCrate.Data;
using MiaCrate.Server.Levels;

namespace MiaCrate.World;

public class ChunkStatus
{
    private const Heightmap.Types PreFeatures = Heightmap.Types.OceanFloorWG | Heightmap.Types.WorldSurfaceWG;

    public const Heightmap.Types PostFeatures = Heightmap.Types.OceanFloor | Heightmap.Types.WorldSurface |
                                                Heightmap.Types.MotionBlocking | Heightmap.Types.MotionBlockingNoLeaves;
    
    private static ILoadingTask PassthroughLoadTask { get; } = 
        ILoadingTask.Create((_, _, _, _, _, chunk) => Task.FromResult(Either.Left<ChunkAccess, ChunkLoadingFailure>(chunk)));

    public static ChunkStatus Empty { get; } = Register("empty", null, -1, PreFeatures, ChunkType.ProtoChunk,
        ISimpleGenerationTask.Create((_, _, _, _, _) => { }));

    public static ChunkStatus Full => throw new NotImplementedException();
    
    private readonly ChunkStatus _parent;
    private readonly int _range;
    private readonly bool _hasLoadDependencies;
    private readonly Heightmap.Types _heightmapsAfter;
    private readonly ChunkType _chunkType;
    private readonly IGenerationTask _generationTask;
    private readonly ILoadingTask _loadingTask;
    
    public int Index { get; }

    private ChunkStatus(ChunkStatus? parent, int range, bool hasLoadDependencies, Heightmap.Types heightmapsAfter,
        ChunkType chunkType, IGenerationTask generationTask, ILoadingTask loadingTask)
    {
        _parent = parent ?? this;
        _range = range;
        _hasLoadDependencies = hasLoadDependencies;
        _heightmapsAfter = heightmapsAfter;
        _chunkType = chunkType;
        _generationTask = generationTask;
        _loadingTask = loadingTask;
        Index = (parent?.Index ?? -1) + 1;
    }
    
    // private static ChunkStatus RegisterSimple(string name, ChunkStatus? parent, int range, Heightmap.Types heightmapsAfter,
    //     ChunkType chunkType, ISimpleGenerationTask generationTask) =>
    //     Register(name, parent, range, heightmapsAfter, chunkType, generationTask);

    private static ChunkStatus Register(string name, ChunkStatus? parent, int range, Heightmap.Types heightmapsAfter, 
        ChunkType chunkType, IGenerationTask generationTask) => 
        Register(name, parent, range, false, heightmapsAfter, chunkType, generationTask, PassthroughLoadTask);

    private static ChunkStatus Register(string name, ChunkStatus? parent, int range, bool hasLoadDependencies, Heightmap.Types heightmapsAfter,
        ChunkType chunkType, IGenerationTask generationTask, ILoadingTask loadingTask) => 
        Registry.Register(BuiltinRegistries.ChunkStatus, name, 
            new ChunkStatus(parent, range, hasLoadDependencies, heightmapsAfter, chunkType, generationTask, loadingTask));

    public enum ChunkType
    {
        ProtoChunk,
        LevelChunk
    }

    public interface IGenerationTask
    {
        public static IGenerationTask Create(GenerationTaskDelegate func) => new Instance(func);
        
        public Task<IEither<ChunkAccess, ChunkLoadingFailure>> DoWorkAsync(ChunkStatus status, IExecutor executor,
            ServerLevel level, ChunkGenerator generator, StructureTemplateManager manager,
            ThreadedLevelLightEngine engine, Func<ChunkAccess, Task<IEither<ChunkAccess, ChunkLoadingFailure>>> func,
            List<ChunkAccess> list, ChunkAccess chunk);

        private class Instance : IGenerationTask
        {
            private readonly GenerationTaskDelegate _delegate;

            public Instance(GenerationTaskDelegate @delegate)
            {
                _delegate = @delegate;
            }

            public Task<IEither<ChunkAccess, ChunkLoadingFailure>> DoWorkAsync(ChunkStatus status, IExecutor executor, 
                ServerLevel level, ChunkGenerator generator, StructureTemplateManager manager, 
                ThreadedLevelLightEngine engine, Func<ChunkAccess, Task<IEither<ChunkAccess, ChunkLoadingFailure>>> func,
                List<ChunkAccess> list, ChunkAccess chunk) =>
                _delegate(status, executor, level, generator, manager, engine, func, list, chunk);
        }
    }
    
    public interface ILoadingTask
    {
        public static ILoadingTask Create(LoadingTaskDelegate func) => new Instance(func);
        
        public Task<IEither<ChunkAccess, ChunkLoadingFailure>> DoWorkAsync(ChunkStatus status, 
            ServerLevel level, StructureTemplateManager manager, ThreadedLevelLightEngine engine, 
            Func<ChunkAccess, Task<IEither<ChunkAccess, ChunkLoadingFailure>>> func, ChunkAccess chunk);
        
        private class Instance : ILoadingTask
        {
            private readonly LoadingTaskDelegate _delegate;

            public Instance(LoadingTaskDelegate @delegate)
            {
                _delegate = @delegate;
            }

            public Task<IEither<ChunkAccess, ChunkLoadingFailure>> DoWorkAsync(ChunkStatus status, ServerLevel level, StructureTemplateManager manager,
                ThreadedLevelLightEngine engine, Func<ChunkAccess, Task<IEither<ChunkAccess, ChunkLoadingFailure>>> func, ChunkAccess chunk) =>
                _delegate(status, level, manager, engine, func, chunk);
        }
    }
    
    public interface ISimpleGenerationTask : IGenerationTask
    {
        public static ISimpleGenerationTask Create(SimpleGenerationTaskDelegate func) => new Instance(func);
        
        public new Task<IEither<ChunkAccess, ChunkLoadingFailure>> DoWorkAsync(ChunkStatus status, IExecutor executor,
            ServerLevel level, ChunkGenerator generator, StructureTemplateManager manager,
            ThreadedLevelLightEngine engine, Func<ChunkAccess, Task<IEither<ChunkAccess, ChunkLoadingFailure>>> func,
            List<ChunkAccess> list, ChunkAccess chunk)
        {
            DoWork(status, level, generator, list, chunk);
            return Task.FromResult(Either.Left<ChunkAccess, ChunkLoadingFailure>(chunk));
        }

        Task<IEither<ChunkAccess, ChunkLoadingFailure>> IGenerationTask.DoWorkAsync(ChunkStatus status,
            IExecutor executor,
            ServerLevel level, ChunkGenerator generator, StructureTemplateManager manager,
            ThreadedLevelLightEngine engine, Func<ChunkAccess, Task<IEither<ChunkAccess, ChunkLoadingFailure>>> func,
            List<ChunkAccess> list, ChunkAccess chunk) =>
            DoWorkAsync(status, executor, level, generator, manager, engine, func, list, chunk);
        
        public void DoWork(ChunkStatus status, ServerLevel level, ChunkGenerator generator, List<ChunkAccess> chunks, ChunkAccess chunk);

        private class Instance : ISimpleGenerationTask
        {
            private readonly SimpleGenerationTaskDelegate _delegate;

            public Instance(SimpleGenerationTaskDelegate @delegate)
            {
                _delegate = @delegate;
            }

            public void DoWork(ChunkStatus status, ServerLevel level, ChunkGenerator generator, List<ChunkAccess> chunks, ChunkAccess chunk) => 
                _delegate(status, level, generator, chunks, chunk);
        }
    }

    public delegate Task<IEither<ChunkAccess, ChunkLoadingFailure>> GenerationTaskDelegate(ChunkStatus status, IExecutor executor,
        ServerLevel level, ChunkGenerator generator, StructureTemplateManager manager,
        ThreadedLevelLightEngine engine, Func<ChunkAccess, Task<IEither<ChunkAccess, ChunkLoadingFailure>>> func,
        List<ChunkAccess> list, ChunkAccess chunk);
    
    public delegate Task<IEither<ChunkAccess, ChunkLoadingFailure>> LoadingTaskDelegate(ChunkStatus status, 
        ServerLevel level, StructureTemplateManager manager, ThreadedLevelLightEngine engine, 
        Func<ChunkAccess, Task<IEither<ChunkAccess, ChunkLoadingFailure>>> func, ChunkAccess chunk);
    
    public delegate void SimpleGenerationTaskDelegate(ChunkStatus status, ServerLevel level, ChunkGenerator generator, List<ChunkAccess> chunks, ChunkAccess chunk);
}