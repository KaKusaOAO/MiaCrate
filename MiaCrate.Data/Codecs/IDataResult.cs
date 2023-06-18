using MiaCrate.Data.Utils;
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
                r2 => new DataResult<TOut>.PartialResult(() => r.Message + "; " + r2.Message, r2.PartialResultValue)
            )), Lifecycle + second.Lifecycle);
        }).OrElse(() =>
            new DataResult<TOut>(
                Either.Right<TOut, IDataResult<TOut>.IPartialResult>(
                    new DataResult<TOut>.PartialResult(r.MessageFunc, Optional.Empty<TOut>())), Lifecycle))
        );
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

    public static IDataResult<T> Error<T>(Func<string> message) => Error<T>(message, Lifecycle.Experimental);
    public static IDataResult<T> Error<T>(Func<string> message, Lifecycle lifecycle) =>
        new DataResult<T>(Either.Right<T, IDataResult<T>.IPartialResult>(
            new DataResult<T>.PartialResult(message, Optional.Empty<T>())), lifecycle);
}