using MiaCrate.Extensions;
using Mochi.Core;

namespace MiaCrate.Resources;

public interface IResourceManagerReloadListener : IPreparableReloadListener
{
    void OnResourceManagerReload(IResourceManager manager);

    Task IPreparableReloadListener.ReloadAsync(IPreparationBarrier barrier, IResourceManager manager,
        IProfilerFiller profiler, IProfilerFiller profiler2,
        IExecutor executor, IExecutor executor2)
    {
        return barrier.Wait(Unit.Instance).ThenRunAsync(() =>
        {
            profiler2.StartTick();
            profiler2.Push("listener");
            OnResourceManagerReload(manager);
            profiler.Pop();
            profiler2.EndTick();
        }, executor2);
    }
}