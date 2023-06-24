using Mochi.Utils;

namespace MiaCrate.Data.Codecs;

public interface IDataResult : IAppLeft<IDataResult.Mu>
{
    public sealed class Mu : IK1 {} 
    
    public interface IPartialResult
    {
        internal Func<string> MessageFunc { get; }
        string Message { get; }
    }
    
    public Lifecycle Lifecycle { get; }

    IDataResult<TOut> Select<TOut>(Func<object, TOut> func);
}

public interface IDataResult<T> : IDataResult, IApp<IDataResult.Mu, T>
{
    public interface IPartialResult : IDataResult.IPartialResult
    {
        internal IOptional<T> PartialResultValue { get; }
        IDataResult<TOut>.IPartialResult Select<TOut>(Func<T, TOut> func);
    }
    
    IEither<T, IPartialResult> Get();
    IOptional<T> Result => Get().Left;

    IDataResult<TOut> Select<TOut>(Func<T, TOut> func) => new DataResult<TOut>(Get().SelectBoth(func,
        r => (IDataResult<TOut>.IPartialResult)new DataResult<TOut>.PartialResult(r.MessageFunc,
            r.PartialResultValue.Select(func))), Lifecycle);
    IDataResult<TOut> IDataResult.Select<TOut>(Func<object, TOut> func) => Select(t => func(t!));

    IDataResult<TOut> SelectMany<TOut>(Func<T, IDataResult<TOut>> func) => Get().Select(
        l =>
        {
            var second = func(l);
            return new DataResult<TOut>(second.Get(), Lifecycle + second.Lifecycle);
        },
        r => r.PartialResultValue.Select(value =>
        {
            var second = func(value);
            return new DataResult<TOut>(Either.Right<TOut, IDataResult<TOut>.IPartialResult>(second.Get().Select(
                l2 => new DataResult<TOut>.PartialResult(r.MessageFunc, Optional.Of(l2)),
                r2 => new DataResult<TOut>.PartialResult(() => $"{r.Message}; {r2.Message}", r2.PartialResultValue)
            )), Lifecycle + second.Lifecycle);
        }).OrElse(() =>
            new DataResult<TOut>(
                Either.Right<TOut, IDataResult<TOut>.IPartialResult>(
                    new DataResult<TOut>.PartialResult(r.MessageFunc, Optional.Empty<TOut>())), Lifecycle))
        );

    IDataResult<T> SelectError(Func<string, string> func) =>
        new DataResult<T>(Get().SelectRight(r => (IPartialResult)
            new DataResult<T>.PartialResult(() => func(r.MessageFunc()), r.PartialResultValue)), Lifecycle);

    IDataResult<T> SetLifecycle(Lifecycle lifecycle) => new DataResult<T>(Get(), lifecycle);

    IDataResult<TS> Apply2Stable<TR2, TS>(Func<T, TR2, TS> func, IDataResult<TR2> second)
    {
        var instance = DataResultInstance.Shared;
        var f = DataResult.Unbox(instance.Point(func)).SetLifecycle(Lifecycle.Stable);
        return DataResult.Unbox(instance.Boxed().Ap2(f, this, second));
    }

    IDataResult<TR2> Ap<TR2>(IDataResult<Func<T, TR2>> funcResult) =>
        new DataResult<TR2>(Get().Select(
            arg => funcResult.Get().SelectBoth(
                func => func(arg),
                funcErr => (IDataResult<TR2>.IPartialResult) new DataResult<TR2>.PartialResult(funcErr.MessageFunc, 
                    funcErr.PartialResultValue.Select(f => f(arg)))
            ),
            argErr => Either.Right<TR2, IDataResult<TR2>.IPartialResult>(funcResult.Get().Select(
                func => new DataResult<TR2>.PartialResult(argErr.MessageFunc, argErr.PartialResultValue.Select(func)),
                funcErr => new DataResult<TR2>.PartialResult(
                    () => $"{argErr.Message}; {funcErr.Message}",
                    argErr.PartialResultValue.SelectMany(a => funcErr.PartialResultValue.Select(f => f(a)))
                )
            ))
        ), Lifecycle + funcResult.Lifecycle);
}

public sealed class DataResultInstance : IApplicative<IDataResult.Mu, DataResultInstance.Mu>
{
    public static readonly DataResultInstance Shared = new(); 
    
    private DataResultInstance() {}
    
    public sealed class Mu : IApplicative.IMu {}

    public IApp<IDataResult.Mu, TResult> Map<TArg, TResult>(Func<TArg, TResult> func, IApp<IDataResult.Mu, TArg> ts) => 
        DataResult.Unbox(ts).Select(func);

    public IApp<IDataResult.Mu, TValue> Point<TValue>(TValue value) => DataResult.Success(value);

    public Func<IApp<IDataResult.Mu, TArg>, IApp<IDataResult.Mu, TResult>> Lift1<TArg, TResult>(
        IApp<IDataResult.Mu, Func<TArg, TResult>> func) =>
        fa => this.Boxed().Ap(func, fa);

    public IApp<IDataResult.Mu, TResult> Ap<TArg, TResult>(IApp<IDataResult.Mu, Func<TArg, TResult>> func, IApp<IDataResult.Mu, TArg> arg) => 
        DataResult.Unbox(arg).Ap(DataResult.Unbox(func));
}

public class DataResult<T> : IDataResult<T>
{
    public Lifecycle Lifecycle { get; }
    private readonly IEither<T, IDataResult<T>.IPartialResult> _result;

    public class PartialResult : IDataResult<T>.IPartialResult
    {
        public Func<string> MessageFunc { get; }
        public IOptional<T> PartialResultValue { get; }

        public PartialResult(Func<string> messageFunc, IOptional<T> partialResultValue)
        {
            MessageFunc = messageFunc;
            PartialResultValue = partialResultValue;
        }

        public IDataResult<TOut>.IPartialResult Select<TOut>(Func<T, TOut> func) => 
            new DataResult<TOut>.PartialResult(MessageFunc, PartialResultValue.Select(func));

        public string Message => MessageFunc();
    }

    internal DataResult(IEither<T, IDataResult<T>.IPartialResult> result, Lifecycle lifecycle)
    {
        Lifecycle = lifecycle;
        _result = result;
    }

    public IEither<T, IDataResult<T>.IPartialResult> Get() => _result;
}

public static class DataResult
{
    public static IDataResult<T> Unbox<T>(IApp<IDataResult.Mu, T> box) => (IDataResult<T>)box;
    public static IDataResult<T> Success<T>(T result) => Success(result, Lifecycle.Experimental);
    public static IDataResult<T> Success<T>(T result, Lifecycle lifecycle) =>
        new DataResult<T>(Either.Left<T, IDataResult<T>.IPartialResult>(result), lifecycle);
    public static IDataResult<T> Error<T>(Func<string> message) => Error<T>(message, Lifecycle.Experimental);
    public static IDataResult<T> Error<T>(Func<string> message, Lifecycle lifecycle) =>
        new DataResult<T>(Either.Right<T, IDataResult<T>.IPartialResult>(
            new DataResult<T>.PartialResult(message, Optional.Empty<T>())), lifecycle);
}