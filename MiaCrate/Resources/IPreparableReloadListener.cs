namespace MiaCrate.Resources;

public interface IPreparableReloadListener
{
    public Task ReloadAsync(IPreparationBarrier barrier, IResourceManager manager,
        IProfilerFiller profiler, IProfilerFiller profile2,
        IExecutor executor, IExecutor executor2);

    public string Name => GetType().Name;
    
    public interface IPreparationBarrier
    {
        public Task<T> Wait<T>(T obj);
    }
}