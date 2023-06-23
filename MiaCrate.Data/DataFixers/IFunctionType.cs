namespace MiaCrate.Data;

public interface IFunctionType : IFunction, IApp2
{
    public class Mu : IK2 {}
}

public interface IFunctionTypeIn<T> : IFunctionIn<T>, IApp2Left<IFunctionType.Mu>, IApp2FieldA<T>,
    IAppLeft<IFunctionTypeIn<T>.ReaderMu>
{
    public class ReaderMu : IK1 {}
}

public interface IFunctionTypeOut<T> : IFunctionOut<T>, IApp2FieldB<T>, IAppRight<T> { }

public interface IFunctionType<TIn, TOut> : IFunctionTypeIn<TIn>, IFunctionTypeOut<TOut>, IFunction<TIn, TOut>,
    IApp2<IFunctionType.Mu, TIn, TOut>, IApp<IFunctionTypeIn<TIn>.ReaderMu, TOut>
{
    
}

public static class FunctionType
{
    public static IFunctionType<TIn, TOut> Unbox<TIn, TOut>(IApp2<IFunctionType.Mu, TIn, TOut> box) =>
        (IFunctionType<TIn, TOut>)box;
    public static IFunctionType<TIn, TOut> Unbox<TIn, TOut>(IApp<IFunctionTypeIn<TIn>.ReaderMu, TOut> box) =>
        (IFunctionType<TIn, TOut>)box;
    
    
}