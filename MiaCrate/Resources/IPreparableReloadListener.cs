using MiaCrate.Common;

namespace MiaCrate.Resources;

public interface IPreparableReloadListener
{
    public Task ReloadAsync(IPreparationBarrier barrier, IResourceManager manager,
        IProfilerFiller preparationProfiler, IProfilerFiller reloadProfiler,
        IExecutor preparationExecutor, IExecutor reloadExecutor);

    public string Name => GetType().Name;
    
    public interface IPreparationBarrier
    {
        public Task<T> Wait<T>(T obj);
    }
}