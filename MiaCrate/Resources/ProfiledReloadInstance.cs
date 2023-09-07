using System.Diagnostics;
using Mochi.Utils;

namespace MiaCrate.Resources;

public class ProfiledReloadInstance : SimpleReloadInstance<ProfiledReloadInstance.State>
{
    private readonly Stopwatch _total = new();
    
    public ProfiledReloadInstance(IResourceManager manager, List<IPreparableReloadListener> list, IExecutor executor,
        IExecutor executor2, Task task) : base(executor, executor2, manager, list, ProfiledStateFactory, task)
    {
        _total.Start();
        _allDone = _allDone.ContinueWith(t => Finish(t.Result));
    }

    private List<State> Finish(List<State> list)
    {
        _total.Stop();
        var l = 0L;
        Logger.Info($"Resource reload finished after {_total.ElapsedMilliseconds} ms");

        foreach (var state in list)
        {
            var m = state.PreparationNanos / 1000000L;
            var n = state.ReloadNanos / 1000000L;
            var o = m + n;
            Logger.Info($"{state.Name} took approximately {o} ms ({m} ms preparing, {n} ms applying)");
            l += n;
        }
        
        Logger.Info($"Total blocking time: {l} ms");
        return list;
    }

    private static Task<State> ProfiledStateFactory(IPreparableReloadListener.IPreparationBarrier barrier, IResourceManager manager, IPreparableReloadListener listener, IExecutor executor, IExecutor executor2)
    {
        var l1 = 0L;
        var l2 = 0L;
        var profiler = InactiveProfiler.Instance;
        var profiler2 = InactiveProfiler.Instance;
        
        return listener.ReloadAsync(barrier, manager, profiler, profiler2, IExecutor.Create(r =>
            {
                executor.Execute(() =>
                {
                    var l = Util.GetNanos();
                    r.Run();
                    l1 += Util.GetNanos() - l;
                });
            }), IExecutor.Create(r =>
            {
                executor2.Execute(() =>
                {
                    var l = Util.GetNanos();
                    r.Run();
                    l2 += Util.GetNanos() - l;
                });
            })
        ).ContinueWith(_ =>
        {
            Logger.Verbose($"Finished reloading {listener.Name}");
            return new State(listener.Name, profiler.Results, profiler2.Results, l1, l2);
        });
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