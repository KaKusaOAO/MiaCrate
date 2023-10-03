using MiaCrate.Common;
using MiaCrate.Extensions;

namespace MiaCrate.Resources;

public abstract class SimplePreparableReloadListener<T> : IPreparableReloadListener
{
    public Task ReloadAsync(IPreparableReloadListener.IPreparationBarrier barrier, IResourceManager manager,
        IProfilerFiller preparationProfiler, IProfilerFiller reloadProfiler, 
        IExecutor preparationExecutor, IExecutor reloadExecutor)
    {
        return Tasks
            .SupplyAsync(() => Prepare(manager, preparationProfiler), preparationExecutor)
            .ThenComposeAsync(barrier.Wait)
            .ThenAcceptAsync(o => Apply(o, manager, reloadProfiler), reloadExecutor);
    }

    public virtual string Name => GetType().Name;

    protected abstract T Prepare(IResourceManager manager, IProfilerFiller profiler);
    protected abstract void Apply(T obj, IResourceManager manager, IProfilerFiller profiler);
}