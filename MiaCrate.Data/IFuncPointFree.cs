using System.Runtime.CompilerServices;
using Mochi.Utils;
using Timeout = System.Threading.Timeout;

namespace MiaCrate.Data;

public interface IFuncPointFree : IPointFree<IFunction> {}
public interface IFuncPointFreeIn<T> : IFuncPointFree {}
public interface IFuncPointFreeOut<T> : IFuncPointFree {}

public interface IFuncPointFree<TIn, TOut> : IFuncPointFreeIn<TIn>, IFuncPointFreeOut<TOut>,
    IPointFree<IFunction<TIn, TOut>>
{
    public new IType<IFunction<TIn, TOut>> Type { get; }
    IType IPointFree.Type => Type;
    IType<IFunction> IPointFree<IFunction>.Type => (IType<IFunction>)Type;
    IType<IFunction<TIn, TOut>> IPointFree<IFunction<TIn, TOut>>.Type => Type;
    
    public new Func<IDynamicOps, IFunction<TIn, TOut>> EvalCached();
    Func<IDynamicOps, IFunction<TIn, TOut>> IPointFree<IFunction<TIn, TOut>>.EvalCached() => EvalCached();
    Func<IDynamicOps, IFunction> IPointFree<IFunction>.EvalCached() => EvalCached();
    Func<IDynamicOps, object> IPointFree.EvalCached() => EvalCached();
    
    public new Func<IDynamicOps, IFunction<TIn, TOut>> Eval();
    Func<IDynamicOps, IFunction<TIn, TOut>> IPointFree<IFunction<TIn, TOut>>.Eval() => Eval();
    Func<IDynamicOps, IFunction> IPointFree<IFunction>.Eval() => Eval();
    
    public new IOptional<IPointFree<IFunction<TIn, TOut>>> All(IPointFreeRule rule);
    IOptional<IPointFree<IFunction<TIn, TOut>>> IPointFree<IFunction<TIn, TOut>>.All(IPointFreeRule rule) => All(rule);
    IOptional<IPointFree<IFunction>> IPointFree<IFunction>.All(IPointFreeRule rule) => 
        All(rule).Select(e => Unsafe.As<IPointFree<IFunction>>(e));
    
    public new IOptional<IPointFree<IFunction<TIn, TOut>>> One(IPointFreeRule rule);
    IOptional<IPointFree<IFunction<TIn, TOut>>> IPointFree<IFunction<TIn, TOut>>.One(IPointFreeRule rule) => One(rule);
    IOptional<IPointFree<IFunction>> IPointFree<IFunction>.One(IPointFreeRule rule) => 
        One(rule).Select(e => Unsafe.As<IPointFree<IFunction>>(e));
}