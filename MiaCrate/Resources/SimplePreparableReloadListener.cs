using MiaCrate.Extensions;

namespace MiaCrate.Resources;

public abstract class SimplePreparableReloadListener<T> : IPreparableReloadListener
{
    public Task ReloadAsync(IPreparableReloadListener.IPreparationBarrier barrier, IResourceManager manager,
        IProfilerFiller profiler, IProfilerFiller profiler2, 
        IExecutor executor, IExecutor executor2)
    {
        return Tasks
            .SupplyAsync(() => Prepare(manager, profiler), executor)
            .ThenComposeAsync(barrier.Wait)
            .ThenAcceptAsync(o => Apply(o, manager, profiler2), executor2);
    }

    public virtual string Name => GetType().Name;

    protected abstract T Prepare(IResourceManager manager, IProfilerFiller profiler);
    protected abstract void Apply(T obj, IResourceManager manager, IProfilerFiller profiler);
}