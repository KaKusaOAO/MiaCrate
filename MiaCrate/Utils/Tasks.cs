using Mochi.Core;

namespace MiaCrate;

public static class Tasks
{
    public static Task<T> SupplyAsync<T>(Func<T> func, IExecutor executor)
    {
        var source = new TaskCompletionSource<T>();
        executor.Execute(() =>
        {
            try
            {
                source.SetResult(func());
            }
            catch (Exception ex)
            {
                TrySetException(source, ex);
            }
        });
        return source.Task;
    }
    
    public static Task RunAsync(Action func, IExecutor executor)
    {
        var source = new TaskCompletionSource<Unit>();
        executor.Execute(() =>
        {
            try
            {
                func();
                source.SetResult(Unit.Instance);
            }
            catch (Exception ex)
            {
                TrySetException(source, ex);
            }
        });
        return source.Task;
    }
    
    
    public static Task<TOther> ApplyToEitherAsync<T, TOther>(this Task<T> task, Task<T> other, Func<T, TOther> func) => 
        Task.WhenAny(task, other).ThenApplyAsync(t => func(t.Result));

    private static void TrySetException<T>(TaskCompletionSource<T> source, Exception ex)
    {
        if (ex is AggregateException aggregateException)
        {
            var list = aggregateException.InnerExceptions;
            if (list.Count == 1)
                source.TrySetException(list.First());
            else
                source.TrySetException(list);
        }
        else
        {
            source.TrySetException(ex);
        }
    }
    
    private static void TryRun<T>(TaskCompletionSource<T> source, Func<T> func)
    {
        try
        {
            source.SetResult(func());
        }
        catch (Exception ex)
        {
            TrySetException(source, ex);
        }
    }
    
    public static Task ThenRunAsync(this Task task, Action action)
    {
        var source = new TaskCompletionSource<Unit>();
        task.ContinueWith(t =>
        {
            TryRun(source, () =>
            {
                // Ensure the errors in the previous task is handled
                t.Wait();

                // Run the action
                action();
                return Unit.Instance;
            });
        });

        return source.Task;
    }
    
    public static Task ThenRunAsync(this Task task, Action action, IExecutor executor)
    {
        var source = new TaskCompletionSource<Unit>();
        task.ContinueWith(t =>
        {
            RunAsync(() =>
            {
                // Ensure the errors in the previous task is handled
                t.Wait();

                // Run the action
                action();
                source.SetResult(Unit.Instance);
            }, executor);
        });

        return source.Task;
    }

    public static Task<T> ThenApplyAsync<T>(this Task task, Func<T> func)
    {
        var source = new TaskCompletionSource<T>();
        task.ContinueWith(t =>
        {
            TryRun(source, () =>
            {
                // Ensure the errors in the previous task is handled
                t.Wait();
                return func();
            });
        });

        return source.Task;
    }
    
    public static Task<T> ThenApplyAsync<TResult, T>(this Task<TResult> task, Func<TResult, T> func)
    {
        var source = new TaskCompletionSource<T>();
        task.ContinueWith(t =>
        {
            TryRun(source, () => func(t.Result));
        });

        return source.Task;
    }

    public static Task<T> ThenApplyAsync<T>(this Task task, Func<T> func, IExecutor executor)
    {
        var source = new TaskCompletionSource<T>();
        task.ContinueWith(t =>
        {
            RunAsync(() =>
            {
                TryRun(source, () =>
                {
                    // Ensure the errors in the previous task is handled
                    t.Wait();
                    return func();
                });
            }, executor);
        });

        return source.Task;
    }

    public static Task ThenAcceptAsync<T>(this Task<T> task, Action<T> func, IExecutor executor)
    {
        var source = new TaskCompletionSource<Unit>();
        task.ContinueWith(t =>
        {
            RunAsync(() =>
            {
                TryRun(source, () =>
                {
                    func(t.Result);
                    return Unit.Instance;
                });
            }, executor);
        });

        return source.Task;
    }
    
    public static Task<T> ThenApplyAsync<TResult, T>(this Task<TResult> task, Func<TResult, T> func, IExecutor executor)
    {
        var source = new TaskCompletionSource<T>();
        task.ContinueWith(t =>
        {
            RunAsync(() =>
            {
                TryRun(source, () => func(t.Result));
            }, executor);
        });

        return source.Task;
    }
    
    public static Task<T> ThenCombineAsync<TA, TB, T>(this Task<TA> task, Task<TB> other, Func<TA, TB, T> func, IExecutor executor) => 
        task.ThenComposeAsync(a => 
            other.ThenComposeAsync(b => 
                SupplyAsync(() => func(a, b), executor)));

    public static Task<T> ThenCombineAsync<TA, TB, T>(this Task<TA> task, Task<TB> other, Func<TA, TB, T> func) => 
        task.ThenComposeAsync(a => 
            other.ThenApplyAsync(b => func(a, b)));

    public static Task<T> ThenComposeAsync<TResult, T>(this Task<TResult> task, Func<TResult, Task<T>> func)
    {
        var source = new TaskCompletionSource<T>();
        task.ContinueWith(t =>
        {
            try
            {
                func(t.Result).ContinueWith(t2 =>
                {
                    TryRun(source, () => t2.Result);
                });
            }
            catch (Exception ex)
            {
                TrySetException(source, ex);
            }
        });

        return source.Task;
    }
}