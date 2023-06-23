namespace MiaCrate.Data;

public interface IFunction
{
    public object? Apply(object? input);
}

public interface IFunctionIn<T> : IFunction {}
public interface IFunctionOut<T> : IFunction {}

public interface IFunction<TIn, TOut> : IFunctionIn<TIn>, IFunctionOut<TOut>
{
    public TOut Apply(TIn input);
    object? IFunction.Apply(object? input) => Apply((TIn)input!);
}

public static class Function
{
    public static IFunction<TIn, TOut> Create<TIn, TOut>(Func<TIn, TOut> func) => 
        new DelegateFunction<TIn, TOut>(func);
    
    private class DelegateFunction<TIn, TOut> : IFunction<TIn, TOut>
    {
        private readonly Func<TIn, TOut> _func;

        public DelegateFunction(Func<TIn, TOut> func)
        {
            _func = func;
        }

        public TOut Apply(TIn input) => _func(input);
    }
}