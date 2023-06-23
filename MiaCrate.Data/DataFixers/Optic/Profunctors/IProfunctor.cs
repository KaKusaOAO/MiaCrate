namespace MiaCrate.Data.Optic;

public interface IProfunctor : IKind2
{
    public interface IMu : IKind2.IMu { }
}
public interface IProfunctorLeft<T> : IProfunctor, IKind2Left<T> where T : IK2 { }
public interface IProfunctorRight<T> : IProfunctor, IKind2Right<T> where T : IProfunctor.IMu { }

public interface IProfunctor<TLeft, TRight> : IProfunctorLeft<TLeft>, IProfunctorRight<TRight>, IKind2<TLeft, TRight> 
    where TLeft : IK2 where TRight : IProfunctor.IMu
{
    
}
