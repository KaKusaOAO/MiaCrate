using System.Collections.Concurrent;
using Mochi.Utils;

namespace MiaCrate.Client.Utils;

public abstract class BlockableEventLoop<T> : IExecutor, IProcessorHandle<T> where T : IRunnable
{
    public string Name { get; }
    protected abstract Thread RunningThread { get; }
    public bool IsSameThread => Thread.CurrentThread == RunningThread;
    private readonly ConcurrentQueue<T> _pendingRunnables = new();
    private int _blockingCount;

    protected BlockableEventLoop(string name)
    {
        Name = name;
    }

    protected abstract T WrapRunnable(IRunnable runnable);
    protected abstract bool ShouldRun(T runnable);

    public bool PollTask()
    {
        if (!_pendingRunnables.TryPeek(out var runnable)) return false;
        if (_blockingCount == 0 && !ShouldRun(runnable)) return false;
        if (!_pendingRunnables.TryDequeue(out runnable)) return false;
        
        DoRunTask(runnable);
        return true;
    }

    protected void RunAllTasks()
    {
        while (PollTask())
        {
            // ...
        }
    }

    protected void DropAllTasks() => _pendingRunnables.Clear();

    public void ExecuteIfPossible(IRunnable runnable) => Execute(runnable);

    private Task InternalSubmitAsync(IRunnable runnable)
    {
        var source = new TaskCompletionSource();
        Execute(IRunnable.Create(() =>
        {
            runnable.Run();
            source.SetResult();
        }));
        return source.Task;
    }

    public Task SubmitAsync(IRunnable runnable)
    {
        if (ScheduleExecutables())
        {
            return InternalSubmitAsync(runnable);
        }
        
        runnable.Run();
        return Task.CompletedTask;
    }

    public Task<T> SubmitAsync(Func<T> func)
    {
        if (ScheduleExecutables())
        {
            var source = new TaskCompletionSource<T>();
            Execute(IRunnable.Create(() =>
            {
                source.SetResult(func());
            }));
            return source.Task;
        }

        return Task.FromResult(func());
    }
    
    public void ExecuteBlocking(IRunnable runnable)
    {
        if (!IsSameThread)
        {
            InternalSubmitAsync(runnable).Wait();
        }
        else
        {
            runnable.Run();
        }
    }

    public void Tell(T runnable)
    {
        _pendingRunnables.Enqueue(runnable);
        Thread.Yield();
    }
    
    public void Execute(IRunnable runnable)
    {
        if (ScheduleExecutables())
        {
            Tell(WrapRunnable(runnable));
        }
        else
        {
            runnable.Run();
        }
    }
    
    public void Execute(Action action) => Execute(IRunnable.Create(action));

    protected virtual bool ScheduleExecutables() => !IsSameThread;

    protected virtual void DoRunTask(T runnable)
    {
        try
        {
            runnable.Run();
        }
        catch (Exception ex)
        {
            Logger.Error($"Error executing task on {Name}");
            Logger.Error(ex);
        }
    }

    public virtual void Dispose()
    {
        
    }
}