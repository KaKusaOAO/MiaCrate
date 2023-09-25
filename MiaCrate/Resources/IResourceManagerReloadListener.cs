using MiaCrate.Extensions;
using Mochi.Core;

namespace MiaCrate.Resources;

public interface IResourceManagerReloadListener : IPreparableReloadListener
{
    void OnResourceManagerReload(IResourceManager manager);

    Task IPreparableReloadListener.ReloadAsync(IPreparationBarrier barrier, IResourceManager manager,
        IProfilerFiller preparationProfiler, IProfilerFiller reloadProfiler,
        IExecutor preparationExecutor, IExecutor reloadExecutor)
    {
        return barrier.Wait(Unit.Instance).ThenRunAsync(() =>
        {
            reloadProfiler.StartTick();
            reloadProfiler.Push("listener");
            OnResourceManagerReload(manager);
            preparationProfiler.Pop();
            reloadProfiler.EndTick();
        }, reloadExecutor);
    }
}