using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MiaCrate.Common;
using MiaCrate.Core;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Extensions;
using MiaCrate.Texts;
using Mochi.Texts;
using Mochi.Utils;
using TextExtension = Mochi.Texts.TextExtension;

namespace MiaCrate;

public static partial class Util
{
    public static INanoTimeSource TimeSource { get; set; } = INanoTimeSource.Create(() => NanoTime);
    public static long NanoTime => 100 * Stopwatch.GetTimestamp();
    public static long MillisTime => NanoTime / 1000000L;
    public static long EpochMillis => DateTimeOffset.Now.ToUnixTimeMilliseconds();

    public static IExecutor BackgroundExecutor { get; } = new TaskExecutor();

    public static long GetNanos() => TimeSource.GetNanos();
    public static long GetMillis() => GetNanos() / 1000000L;
    
    public static T Make<T>(Func<T> supplier) => supplier();
    public static T Make<T>(T thing, Action<T> modifier)
    {
        modifier(thing);
        return thing;
    }

    public static ArraySegment<T> CreateSegment<T>(T[] array, int startInclusive, int endExclusive)
    {
        var count = endExclusive - startInclusive;
        return new ArraySegment<T>(array, startInclusive, count);
    }

    public static T[] SubArray<T>(T[] array, int startInclusive, int endExclusive) =>
        CreateSegment(array, startInclusive, endExclusive).ToArray();

    public static bool IsBlank(this string? str) => string.IsNullOrEmpty(str) || str.All(c => c.IsWhitespace());

    public static bool IsWhitespace(this char c) => char.IsWhiteSpace(c);

    public static int NumberOfTrailingZeros(int i)
    {
        i = ~i & (i - 1);
        if (i <= 0) return i & 32;

        var n = 1;
        if (i > 1 << 16) { n += 16; i >>>= 16; }
        if (i > 1 <<  8) { n +=  8; i >>>=  8; }
        if (i > 1 <<  4) { n +=  4; i >>>=  4; }
        if (i > 1 <<  2) { n +=  2; i >>>=  2; }
        return n + (i >>> 1);
    }

    public static int NumberOfTrailingZeros(long l)
    {
        var x = (int) l;
        return x == 0 ? 32 + NumberOfTrailingZeros((int) (l >>> 32)) : NumberOfTrailingZeros(x);
    }
    
    public static int NumberOfTrailingZeros(uint i)
    {
        i = ~i & (i - 1);
        if (i <= 0) return (int) (i & 32);

        var n = 1u;
        if (i > 1 << 16) { n += 16; i >>>= 16; }
        if (i > 1 <<  8) { n +=  8; i >>>=  8; }
        if (i > 1 <<  4) { n +=  4; i >>>=  4; }
        if (i > 1 <<  2) { n +=  2; i >>>=  2; }
        return (int) (n + (i >>> 1));
    }

    public static int NumberOfTrailingZeros(ulong l)
    {
        var x = (uint) l;
        return x == 0 ? 32 + NumberOfTrailingZeros((uint) (l >>> 32)) : NumberOfTrailingZeros(x);
    }

    public static Task<List<T>> Sequence<T>(List<Task<T>> list)
    {
        if (!list.Any()) return Task.FromResult(new List<T>());
        return list.Count == 1
            ? list.First().ThenApplyAsync(t => new List<T> {t})
            : Task.WhenAll(list).ThenApplyAsync(t => t.ToList());
    }

    public static Task<List<T>> SequenceFailFast<T>(List<Task<T>> list)
    {
        var source = new TaskCompletionSource<List<T>>();
        return FallibleSequence(list, source.SetException)
            .ApplyToEitherAsync(source.Task, t => t);
    }

    public static void LogAndPauseIfInIde(object message)
    {
        Logger.Error(message);
        Debugger.Break();
    }

    public static void LogFoobar(
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1,
        [CallerMemberName] string methodName = "")
    {
        Logger.Warn(MiaComponent.Literal("")
            .AddExtra(MiaComponent.Literal(filePath)
                .SetColor(TextColor.Aqua))
            .AddExtra(MiaComponent.Literal("::")
                .SetColor(TextColor.Gray))
            .AddExtra(MiaComponent.Literal(methodName)
                .SetColor(TextColor.Gold))
            .AddExtra(MiaComponent.Translatable("(%s)")
                .SetColor(TextColor.Gray)
                .AppendWith(MiaComponent.Literal("...")
                    .SetColor(TextColor.DarkGray)
                ))
            .AddExtra(MiaComponent.Literal($" (line {lineNumber})")
                .SetColor(TextColor.DarkGray))
            .AddExtra(MiaComponent.Literal(" is foobar!"))
        );
    }

    public static Task<List<T>> FallibleSequence<T>(List<Task<T>> list, Action<Exception> handler)
    {
        var list2 = new List<T>(list.Count);
        var wait = new Task[list.Count];
        foreach (var task in list)
        {
            var i = list2.Count;
            list2.Add(default!);
            wait[i] = task.ContinueWith(t =>
            {
                try
                {
                    if (t.IsCanceled)
                    {
                        handler(new TaskCanceledException(t));
                        return;
                    }

                    if (t.IsFaulted)
                    {
                        var ex = t.Exception!;
                        var exceptions = ex.InnerExceptions;
                        if (exceptions.Count == 1)
                            handler(exceptions.First());
                        else
                            handler(ex);
                        return;
                    }

                    list2[i] = t.Result;
                }
                catch (Exception ex)
                {
                    Logger.Warn("Unexpected error occurred while handling completed tasks");
                    Logger.Warn(ex);
                }
            });
        }

        return Task.WhenAll(wait).ThenApplyAsync(() => list2);
    }

    public static int LowestOneBit(int i) => i & -i;

    public static IDataResult<List<T>> FixedSize<T>(List<T> list, int size)
    {
        if (list.Count == size) return DataResult.Success(list);
        
        string ErrorMessage() => $"Input is not a list of {size} elements";
        return list.Count >= size
            ? DataResult.Error(ErrorMessage, list.Take(size).ToList())
            : DataResult.Error<List<T>>(ErrorMessage);
    }

    public static T GetOrThrow<T>(IDataResult<T> result, Func<string, Exception> ex)
    {
        var optional = result.Error;
        if (optional.IsPresent) throw ex(optional.Value.Message);
        return result.Result.OrElseGet(() => throw new InvalidOperationException());
    }

    public static Func<T, TR> Memoize<T, TR>(Func<T, TR> func) where T : notnull
    {
        var dict = new Dictionary<T, TR>();
        return a => dict.ComputeIfAbsent(a, _ => func(a));
    }
    
    public static Func<TA, TB, TR> Memoize<TA, TB, TR>(Func<TA, TB, TR> func)
    {
        var dict = new Dictionary<(TA, TB), TR>();
        return (a, b) => dict.ComputeIfAbsent((a, b), _ => func(a, b));
    }
    
    public static T GetRandom<T>(List<T> contents, IRandomSource randomSource) => 
        contents[randomSource.Next(contents.Count)];

    public static IOptional<T> GetRandomSafe<T>(List<T> contents, IRandomSource randomSource) => 
        !contents.Any() 
            ? Optional.Empty<T>() 
            : Optional.Of(GetRandom(contents, randomSource));

    private class TaskExecutor : IExecutor
    {
        private static readonly ConcurrentQueue<IRunnable> _queue = new();

        public TaskExecutor()
        {
            _ = RunEventLoopAsync();
        }

        private async Task RunEventLoopAsync()
        {
            while (true)
            {
                await Task.Yield();
                IRunnable runnable = null!;
                SpinWait.SpinUntil(() => _queue.TryDequeue(out runnable));

                void Run(IRunnable r)
                {
                    // Task.Run(() =>
                    // {
                        try
                        {
                            r.Run();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"--> Unhandled exception in: {r}");
                            Logger.Error(ex);
                        }
                    // });
                }

                Run(runnable);
                
                while (_queue.TryDequeue(out runnable))
                {
                    Run(runnable);
                }
            }
            
            // ReSharper disable once FunctionNeverReturns
        }

        public void Execute(IRunnable runnable)
        {
            _queue.Enqueue(runnable);
        }
    }

    public static IType? FetchChoiceType(Dsl.ITypeReference typeRef, string name)
    {
        return !SharedConstants.CheckDataFixerSchema
            ? null
            : DoFetchChoiceType(typeRef, name);
    }

    private static IType? DoFetchChoiceType(Dsl.ITypeReference typeRef, string name)
    {
        LogFoobar();
        return null;
    }
}