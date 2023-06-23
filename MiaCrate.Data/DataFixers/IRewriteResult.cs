using System.Collections;

namespace MiaCrate.Data;

public interface IRewriteResult {}
public interface IRewriteResultLeft<T> : IRewriteResult {}
public interface IRewriteResultRight<T> : IRewriteResult {}

public interface IRewriteResult<TLeft, TRight> : IRewriteResultLeft<TLeft>, IRewriteResultRight<TRight>
{
    public IView<TLeft, TRight> View { get; }
    public BitArray RecData { get; }
    public IRewriteResult<TOuter, TRight> Compose<TOuter>(IRewriteResult<TOuter, TLeft> that);
}

public record RewriteResult<TLeft, TRight>(IView<TLeft, TRight> View, BitArray RecData) : IRewriteResult<TLeft, TRight>
{
    public IRewriteResult<TOuter, TRight> Compose<TOuter>(IRewriteResult<TOuter, TLeft> that)
    {
        BitArray newData;
        if (View.Type is RecursivePoint.IRecursivePointType && that.View.Type is RecursivePoint.IRecursivePointType)
        {
            newData = new BitArray(RecData);
            newData.Or(that.RecData);
        }
        else
        {
            newData = RecData;
        }

        return new RewriteResult<TOuter, TRight>(View.Compose(that.View), newData);
    }

    public override string ToString() => $"RR[{View}]";
}

public static class RewriteResult
{
    public static IRewriteResult<TLeft, TRight> Create<TLeft, TRight>(IView<TLeft, TRight> view, BitArray recData) => 
        new RewriteResult<TLeft, TRight>(view, recData);

    public static IRewriteResult<T, T> Nop<T>(IType<T> type) =>
        new RewriteResult<T, T>(View.NopView(type), new BitArray(0));
}