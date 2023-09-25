using MiaCrate.Extensions;
using Mochi.Core;

namespace MiaCrate.Resources;

public static class SimpleReloadInstance
{
    public static SimpleReloadInstance<Unit> Of(IResourceManager manager, List<IPreparableReloadListener> list,
        IExecutor preparationExecutor, IExecutor reloadExecutor, Task task) =>
        SimpleReloadInstance<Unit>.Of(manager, list, preparationExecutor, reloadExecutor, task, () => Unit.Instance);
    
    public static IReloadInstance Create(IResourceManager manager, List<IPreparableReloadListener> list,
        IExecutor preparationExecutor, IExecutor reloadExecutor, Task task, bool bl)
    {
        return bl
            ? new ProfiledReloadInstance(manager, list, preparationExecutor, reloadExecutor, task)
            : Of(manager, list, preparationExecutor, reloadExecutor, task);
    }
}

public class SimpleReloadInstance<T> : IReloadInstance
{
    // A state factory will call ReloadAsync() of all the underlying reload listeners.
    // Running this delegate will start the reload and returns a state about the reload on completed.
    protected delegate Task<T> StateFactory(
        IPreparableReloadListener.IPreparationBarrier barrier,
        IResourceManager manager,
        IPreparableReloadListener listener,
        IExecutor preparationExecutor, IExecutor reloadExecutor);

    protected readonly TaskCompletionSource<Unit> _allPreparations = new();
    protected Task<List<T>> _allDone;
    private readonly int _listenerCount;
    private int _startedTaskCounter;
    private int _doneTaskCounter;
    private int _startedReloads;
    private int _finishedReloads;
    protected readonly HashSet<IPreparableReloadListener> _preparingListeners;

    protected SimpleReloadInstance(IExecutor preparationExecutor, IExecutor reloadExecutor, IResourceManager manager,
        List<IPreparableReloadListener> list, StateFactory stateFactory, Task task)
    {
        _listenerCount = list.Count;
        _startedTaskCounter++;
        task.ThenRunAsync(() => _doneTaskCounter++);

        var list2 = new List<Task<T>>();
        var prevTask = task;
        _preparingListeners = new HashSet<IPreparableReloadListener>(list);

        foreach (var listener in list)
        {
            var task4 = stateFactory(new Barrier(this, reloadExecutor, listener, prevTask),
                manager, listener,
                IExecutor.Create(r =>
                {
                    _startedTaskCounter++;
                    preparationExecutor.Execute(() =>
                    {
                        r.Run();
                        _doneTaskCounter++;
                    });
                }),
                IExecutor.Create(r =>
                {
                    ++_startedReloads;
                    reloadExecutor.Execute(() =>
                    {
                        r.Run();
                        ++_finishedReloads;
                    });
                })
            );
            list2.Add(task4);
            prevTask = task4;
        }

        _allDone = Util.SequenceFailFast(list2);
    }

    private class Barrier : IPreparableReloadListener.IPreparationBarrier
    {
        private readonly SimpleReloadInstance<T> _instance;
        private readonly IExecutor _reloadExecutor;
        private readonly IPreparableReloadListener _listener;
        private readonly Task _task2;

        public Barrier(SimpleReloadInstance<T> instance, IExecutor reloadExecutor, IPreparableReloadListener listener,
            Task task2)
        {
            _instance = instance;
            _reloadExecutor = reloadExecutor;
            _listener = listener;
            _task2 = task2;
        }

        public Task<TObj> Wait<TObj>(TObj obj)
        {
            _reloadExecutor.Execute(() =>
            {
                _instance._preparingListeners.Remove(_listener);
                if (!_instance._preparingListeners.Any())
                {
                    _instance._allPreparations.SetResult(Unit.Instance);
                }
            });

            return _instance._allPreparations.Task.ThenCombineAsync(
                _task2.ThenApplyAsync(() => Unit.Instance), 
                (_, _) => obj);
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
        IExecutor preparationExecutor, IExecutor reloadExecutor, Task task, Func<T> continuation)
    {
        return new SimpleReloadInstance<T>(preparationExecutor, reloadExecutor, manager, list,
            (barrier, resourceManager, listener, _, re) =>
            {
                return listener.ReloadAsync(barrier, resourceManager, 
                    InactiveProfiler.Instance, InactiveProfiler.Instance,
                    preparationExecutor, re)
                    .ThenApplyAsync(continuation);
            }, task);
    }
}