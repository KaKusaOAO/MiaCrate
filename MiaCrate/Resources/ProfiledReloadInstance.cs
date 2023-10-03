using System.Diagnostics;
using MiaCrate.Common;
using Mochi.Utils;

namespace MiaCrate.Resources;

public class ProfiledReloadInstance : SimpleReloadInstance<ProfiledReloadInstance.State>
{
    private readonly Stopwatch _total = new();
    
    public ProfiledReloadInstance(IResourceManager manager, List<IPreparableReloadListener> list, 
        IExecutor preparationExecutor, IExecutor reloadExecutor, Task task) 
        : base(preparationExecutor, reloadExecutor, manager, list, PerformReloadAsync, task)
    {
        _total.Start();
        _allDone = _allDone.ThenApplyAsync(Finish, reloadExecutor);
    }

    private List<State> Finish(List<State> list)
    {
        _total.Stop();
        var blockingTime = 0L;
        Logger.Info($"Resource reload finished after {_total.ElapsedMilliseconds} ms");

        foreach (var state in list)
        {
            var preparationMillis = state.PreparationNanos / 1000000L;
            var reloadMillis = state.ReloadNanos / 1000000L;
            var totalMillis = preparationMillis + reloadMillis;
            Logger.Info($"{state.Name} took approximately {totalMillis} ms ({preparationMillis} ms preparing, {reloadMillis} ms applying)");
            blockingTime += reloadMillis;
        }
        
        Logger.Info($"Total blocking time: {blockingTime} ms");
        return list;
    }

    private static Task<State> PerformReloadAsync(IPreparableReloadListener.IPreparationBarrier barrier,
        IResourceManager manager, IPreparableReloadListener listener, IExecutor preparationExecutor, IExecutor reloadExecutor)
    {
        var preparationNanos = 0L;
        var reloadNanos = 0L;
        var preparationProfiler = new ActiveProfiler(Util.TimeSource.GetNanos, () => 0, false);
        var reloadProfiler = new ActiveProfiler(Util.TimeSource.GetNanos, () => 0, false);
        
        return listener
            .ReloadAsync(barrier, manager, preparationProfiler, reloadProfiler,
                // Wrap the preparation executor
                IExecutor.Create(r =>
                {
                    preparationExecutor.Execute(() =>
                    {
                        var l = Util.GetNanos();
                        r.Run();
                        preparationNanos += Util.GetNanos() - l;
                    });
                }),
                // Wrap the reload executor
                IExecutor.Create(r =>
                {
                    reloadExecutor.Execute(() =>
                    {
                        var l = Util.GetNanos();
                        r.Run();
                        reloadNanos += Util.GetNanos() - l;
                    });
                })
            )
            .ThenApplyAsync(() =>
            {
                Logger.Verbose($"Finished reloading {listener.Name}");
                return new State(listener.Name, preparationProfiler.Results, reloadProfiler.Results, 
                    preparationNanos, reloadNanos);
            }, reloadExecutor);
    }

    public class State
    {
        public State(string name, IProfileResults preparationResult, IProfileResults reloadResult, long preparationNanos, long reloadNanos)
        {
            Name = name;
            PreparationResult = preparationResult;
            ReloadResult = reloadResult;
            PreparationNanos = preparationNanos;
            ReloadNanos = reloadNanos;
        }

        public string Name { get; }
        public IProfileResults PreparationResult { get; }
        public IProfileResults ReloadResult { get; }
        public long PreparationNanos { get; }
        public long ReloadNanos { get; }
    }
}