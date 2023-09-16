namespace MiaCrate.Resources;

public abstract class SimplePreparableReloadListener<T> : IPreparableReloadListener
{
    public Task ReloadAsync(IPreparableReloadListener.IPreparationBarrier barrier, IResourceManager manager, IProfilerFiller profiler,
        IProfilerFiller profile2, IExecutor executor, IExecutor executor2)
    {
        var source = new TaskCompletionSource<T>();
        executor.Execute(() =>
        {
            var result = Prepare(manager, profiler);
            source.SetResult(result);
        });

        return source.Task.ContinueWith(t =>
            {
                barrier.Wait(t).Wait();
                return t.Result;
            })
            .ContinueWith(t => Apply(t.Result, manager, profile2));
    }

    public virtual string Name => GetType().Name;

    protected abstract T Prepare(IResourceManager manager, IProfilerFiller profiler);
    protected abstract void Apply(T obj, IResourceManager manager, IProfilerFiller profiler);
}