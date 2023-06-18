using Mochi.Utils;

namespace MiaCrate.Data.Utils;

public interface IEither : IApp
{
    
}

public interface IEitherLeft<T> : IEither
{
    IOptional<T> Left { get; }
}

public interface IEitherRight<T> : IEither, IAppLeft<IEitherRight<T>.Mu>
{
    public sealed class Mu : IK1 {}
    IOptional<T> Right { get; }
}

public interface IEither<TLeft, TRight> : IApp<IEitherRight<TRight>.Mu, TLeft>,
    IEitherLeft<TLeft>, IEitherRight<TRight>
{
    T Select<T>(Func<TLeft, T> l, Func<TRight, T> r);
    IEither<TLeft, TRight> IfLeft(Action<TLeft> consumer);
    IEither<TLeft, TRight> IfRight(Action<TRight> consumer);
    IEither<TRight, TLeft> Swap() => Select(Either.Right<TRight, TLeft>, Either.Left<TRight, TLeft>);
    IEither<T, TRight> SelectLeft<T>(Func<TLeft, T> l) => Select(t => Either.Left<T, TRight>(l(t)), Either.Right<T, TRight>);
    IEither<TLeft, T> SelectRight<T>(Func<TRight, T> r) => Select(Either.Left<TLeft, T>, t => Either.Right<TLeft, T>(r(t)));
    IEither<TA, TB> SelectBoth<TA, TB>(Func<TLeft, TA> l, Func<TRight, TB> r);
}

public abstract class Either<TLeft, TRight> : IEither<TLeft, TRight>
{
    public abstract T Select<T>(Func<TLeft, T> l, Func<TRight, T> r);
    public abstract IEither<TLeft, TRight> IfLeft(Action<TLeft> consumer);
    public abstract IEither<TLeft, TRight> IfRight(Action<TRight> consumer);
    public abstract IEither<TA, TB> SelectBoth<TA, TB>(Func<TLeft, TA> l, Func<TRight, TB> r);

    public TLeft OrThrow() => Select(l => l, r =>
    {
        if (r is Exception ex) throw ex;
        throw new Exception(r?.ToString());
    });

    public abstract IOptional<TLeft> Left { get; }
    public abstract IOptional<TRight> Right { get; }
}

public static class Either
{
    private class LeftValue<TLeft, TRight> : Either<TLeft, TRight>
    {
        private readonly TLeft _value;

        public LeftValue(TLeft value)
        {
            _value = value;
        }

        public override T Select<T>(Func<TLeft, T> l, Func<TRight, T> r) => l(_value);

        public override IEither<TLeft, TRight> IfLeft(Action<TLeft> consumer)
        {
            consumer(_value);
            return this;
        }

        public override IEither<TLeft, TRight> IfRight(Action<TRight> consumer) => this;
        public override IEither<TA, TB> SelectBoth<TA, TB>(Func<TLeft, TA> l, Func<TRight, TB> r) => 
            new LeftValue<TA, TB>(l(_value));

        public override IOptional<TLeft> Left => Optional.Of(_value);
        public override IOptional<TRight> Right => Optional.Empty<TRight>();
    }
    
    private class RightValue<TLeft, TRight> : Either<TLeft, TRight>
    {
        private readonly TRight _value;

        public RightValue(TRight value)
        {
            _value = value;
        }

        public override T Select<T>(Func<TLeft, T> l, Func<TRight, T> r) => r(_value);
        public override IEither<TLeft, TRight> IfLeft(Action<TLeft> consumer) => this;
        public override IEither<TLeft, TRight> IfRight(Action<TRight> consumer)
        {
            consumer(_value);
            return this;
        }

        public override IEither<TA, TB> SelectBoth<TA, TB>(Func<TLeft, TA> l, Func<TRight, TB> r) => 
            new RightValue<TA, TB>(r(_value));

        public override IOptional<TLeft> Left => Optional.Empty<TLeft>();
        public override IOptional<TRight> Right => Optional.Of(_value);
    }

    public static IEither<TLeft, TRight> Left<TLeft, TRight>(TLeft value) => new LeftValue<TLeft, TRight>(value);
    public static IEither<TLeft, TRight> Right<TLeft, TRight>(TRight value) => new RightValue<TLeft, TRight>(value);
}