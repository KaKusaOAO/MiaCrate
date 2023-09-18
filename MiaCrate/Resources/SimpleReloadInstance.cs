using MiaCrate.Extensions;
using Mochi.Core;

namespace MiaCrate.Resources;

public static class SimpleReloadInstance
{
    public static SimpleReloadInstance<Unit> Of(IResourceManager manager, List<IPreparableReloadListener> list,
        IExecutor executor, IExecutor executor2, Task task) =>
        SimpleReloadInstance<Unit>.Of(manager, list, executor, executor2, task, () => Unit.Instance);
    
    public static IReloadInstance Create(IResourceManager manager, List<IPreparableReloadListener> list,
        IExecutor executor, IExecutor executor2, Task task, bool bl)
    {
        return bl
            ? new ProfiledReloadInstance(manager, list, executor, executor2, task)
            : Of(manager, list, executor, executor2, task);
    }
}

public class SimpleReloadInstance<T> : IReloadInstance
{
    protected delegate Task<T> StateFactory(
        IPreparableReloadListener.IPreparationBarrier barrier,
        IResourceManager manager,
        IPreparableReloadListener listener,
        IExecutor executor, IExecutor executor2);

    protected readonly TaskCompletionSource<Unit> _allPreparations = new();
    protected Task<List<T>> _allDone;
    private readonly int _listenerCount;
    private int _startedTaskCounter;
    private int _doneTaskCounter;
    private int _startedReloads;
    private int _finishedReloads;
    protected readonly HashSet<IPreparableReloadListener> _preparingListeners;

    protected SimpleReloadInstance(IExecutor executor, IExecutor executor2, IResourceManager manager,
        List<IPreparableReloadListener> list, StateFactory stateFactory, Task task)
    {
        _listenerCount = list.Count;
        _startedTaskCounter++;
        task.ThenRunAsync(() => _doneTaskCounter++);

        var list2 = new List<Task<T>>();
        var task2 = task;
        _preparingListeners = new HashSet<IPreparableReloadListener>(list);

        foreach (var listener in list)
        {
            var task4 = stateFactory(new Barrier(this, executor2, listener, task2),
                manager, listener,
                IExecutor.Create(r =>
                {
                    _startedTaskCounter++;
                    executor.Execute(() =>
                    {
                        r.Run();
                        _doneTaskCounter++;
                    });
                }),
                IExecutor.Create(r =>
                {
                    ++_startedReloads;
                    executor2.Execute(() =>
                    {
                        r.Run();
                        ++_finishedReloads;
                    });
                })
            );
            list2.Add(task4);
            task2 = task4;
        }

        _allDone = Util.SequenceFailFast(list2);
    }

    private class Barrier : IPreparableReloadListener.IPreparationBarrier
    {
        private readonly SimpleReloadInstance<T> _instance;
        private readonly IExecutor _executor2;
        private readonly IPreparableReloadListener _listener;
        private readonly Task _task2;

        public Barrier(SimpleReloadInstance<T> instance, IExecutor executor2, IPreparableReloadListener listener,
            Task task2)
        {
            _instance = instance;
            _executor2 = executor2;
            _listener = listener;
            _task2 = task2;
        }

        public Task<TObj> Wait<TObj>(TObj obj)
        {
            _executor2.Execute(IRunnable.Create(() =>
            {
                _instance._preparingListeners.Remove(_listener);
                if (!_instance._preparingListeners.Any())
                {
                    _instance._allPreparations.SetResult(Unit.Instance);
                }
            }));

            return _instance._allPreparations.Task.ContinueWith(_ =>
            {
                _task2.Wait();
                return obj;
            });
        }
    }

    public Task Task => _allDone;

    public float ActualProgress
    {
        get
        {
            var i = _listenerCount - _preparingListeners.Count;
            var f = _doneTaskCounter * 2 + _finishedReloads * 2 + i;
            var g = _startedTaskCounter * 2 + _startedReloads * 2 + _listenerCount;
            return (float) f / g;
        }
    }

    public static SimpleReloadInstance<T> Of(IResourceManager manager, List<IPreparableReloadListener> list,
        IExecutor executor, IExecutor executor2, Task task, Func<T> continuation)
    {
        return new SimpleReloadInstance<T>(executor, executor2, manager, list,
            (barrier, resourceManager, listener, _, executor3) =>
                listener.ReloadAsync(barrier, resourceManager, InactiveProfiler.Instance, InactiveProfiler.Instance,
                    executor, executor3).ThenApplyAsync(continuation), task);
    }
}