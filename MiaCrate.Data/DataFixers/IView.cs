using Mochi.Utils;

namespace MiaCrate.Data;

public interface IView : IApp2
{
    public class Mu : IK2 {}
}

public interface IViewLeft<T> : IView {}
public interface IViewRight<T> : IView {}

public interface IView<TLeft, TRight> : IApp2<IView.Mu, TLeft, TRight>, IViewLeft<TLeft>, IViewRight<TRight>
{
    public IPointFree<IFunction<TLeft, TRight>> Function { get; }
    public IType<IFunction<TLeft, TRight>> FuncType { get; }
    public IType<TLeft> Type { get; }
    public IType<TRight> NewType { get; }
    public IOptional<IView<TLeft, TRight>> Rewrite(IPointFreeRule rule);
    public IView<TLeft, TRight> RewriteOrNop(IPointFreeRule rule);
    public IView<TLeft, TOut> SelectMany<TOut>(Func<IType<TRight>, IView<TRight, TOut>> func);
}

public record View<TLeft, TRight>(IPointFree<IFunction<TLeft, TRight>> Function) : IView<TLeft, TRight>
{
    public IType<IFunction<TLeft, TRight>> FuncType => Function.Type;
    public IType<TLeft> Type => ((IFuncTypeIn<TLeft>)FuncType).First;
    public IType<TRight> NewType => ((IFuncTypeOut<TRight>)FuncType).Second;
    
    public IOptional<IView<TLeft, TRight>> Rewrite(IPointFreeRule rule) => 
        rule.Rewrite(Function).Select(f => new View<TLeft, TRight>(f));

    public IView<TLeft, TRight> RewriteOrNop(IPointFreeRule rule) =>
        Rewrite(rule).OrElse(this);

    public IView<TLeft, TOut> SelectMany<TOut>(Func<IType<TRight>, IView<TRight, TOut>> func)
    {
        var instance = func(NewType);
        return new View<TLeft, TOut>(Functions.Comp(instance.Function, Function));
    }
}

public static class View
{
    public static IView<TLeft, TRight> Unbox<TLeft, TRight>(IApp2<IView.Mu, TLeft, TRight> box) =>
        (IView<TLeft, TRight>)box;

    public static IView<T, T> NopView<T>(IType<T> type) => new View<T, T>(Functions.Id(type));

    public static IView<TLeft, TRight> Create<TLeft, TRight>(IPointFree<IFunction<TLeft, TRight>> function) =>
        new View<TLeft, TRight>(function);
}


