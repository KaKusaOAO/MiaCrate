namespace MiaCrate.Common;

#if NET5_0_OR_GREATER
using NoResultTaskCompletionSource = TaskCompletionSource;
#else
using NoResultTaskCompletionSource = TaskCompletionSource<Unit>;
#endif


internal readonly struct Unit
{
    public static Unit Instance { get; } = new();
}

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
    
    public static Task RunAsync(Action action, IExecutor executor)
    {
        var source = new NoResultTaskCompletionSource();
        executor.Execute(() =>
        {
            try
            {
                action();
                NotifyDone(source);
            }
            catch (Exception ex)
            {
                TrySetException(source, ex);
            }
        });
        return source.Task;
    }
    
    private static void TryRun(NoResultTaskCompletionSource source, Action action)
    {
        try
        {
            action();
            NotifyDone(source);
        }
        catch (Exception ex)
        {
            TrySetException(source, ex);
        }
    }
    
    public static Task<TOther> ApplyToEitherAsync<T, TOther>(this Task<T> task, Task<T> other, Func<T, TOther> func) => 
        Task.WhenAny(task, other).ThenApplyAsync(t => func(t.Result));

    private static void TrySetException(NoResultTaskCompletionSource source, Exception ex)
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
    
    public static Task ExceptionallyAsync(this Task task, Action<Exception> action)
    {
        var source = new TaskCompletionSource<Unit>();
        task.ContinueWith(t =>
        {
            TryRun(source, () =>
            {
                try
                {
                    // Ensure the errors in the previous task is handled
                    t.Wait();
                }
                catch (Exception ex)
                {
                    // Handle in the action
                    action(ex);
                }
                
                return Unit.Instance;
            });
        });

        return source.Task;
    }
    
    public static Task<T> ExceptionallyAsync<T>(this Task<T> task, Func<Exception, T> func)
    {
        var source = new TaskCompletionSource<T>();
        task.ContinueWith(t =>
        {
            TryRun(source, () =>
            {
                try
                {
                    // Ensure the errors in the previous task is handled
                    return t.Result;
                }
                catch (Exception ex)
                {
                    // Handle in the action
                    return func(ex);
                }
            });
        });

        return source.Task;
    }
    
    public static Task ThenRunAsync(this Task task, Action action, IExecutor executor)
    {
        var source = new NoResultTaskCompletionSource();
        task.ContinueWith(t =>
        {
            executor.Execute(() =>
            {
                TryRun(source, () =>
                {
                    // Ensure the errors in the previous task is handled
                    t.Wait();

                    // Run the action
                    action();
                    NotifyDone(source);
                });
            });
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
            executor.Execute(() =>
            {
                TryRun(source, () =>
                {
                    // Ensure the errors in the previous task is handled
                    t.Wait();
                    return func();
                });
            });
        });

        return source.Task;
    }

    public static Task ThenAcceptAsync<T>(this Task<T> task, Action<T> func, IExecutor executor)
    {
        var source = new TaskCompletionSource<Unit>();
        task.ContinueWith(t =>
        {
            executor.Execute(() =>
            {
                TryRun(source, () =>
                {
                    func(t.Result);
                    return Unit.Instance;
                });
            });
        });

        return source.Task;
    }
    
    public static Task<T> ThenApplyAsync<TResult, T>(this Task<TResult> task, Func<TResult, T> func, IExecutor executor)
    {
        var source = new TaskCompletionSource<T>();
        task.ContinueWith(t =>
        {
            executor.Execute(() =>
            {
                TryRun(source, () => func(t.Result));
            });
        });

        return source.Task;
    }

    private static void NotifyDone(NoResultTaskCompletionSource source)
    {
        #if NET5_0_OR_GREATER
        source.SetResult();
        #else
        source.SetResult(Unit.Instance);
        #endif
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