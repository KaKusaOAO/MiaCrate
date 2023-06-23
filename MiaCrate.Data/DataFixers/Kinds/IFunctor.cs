namespace MiaCrate.Data;

public interface IFunctor : IKind1
{
    public interface IMu : IKind1.IMu
    {
        
    }
}

public interface IFunctorLeft<T> : IFunctor, IKind1Left<T> where T : IK1
{
    public IApp<T, TResult> Map<TArg, TResult>(Func<TArg, TResult> func, IApp<T, TArg> ts);
}

public interface IFunctorRight<T> : IFunctor, IKind1Right<T> where T : IFunctor.IMu
{
}

public interface IFunctor<TLeft, TRight> : IKind1<TLeft, TRight>, IFunctorLeft<TLeft>, IFunctorRight<TRight>
    where TLeft : IK1 where TRight : IFunctor.IMu
{
}