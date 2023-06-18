namespace MiaCrate.Data;

public interface IKind2 : IApp
{
    public interface IMu : IK1
    {
        
    }
}

public interface IKind2Left<T> : IKind2, IAppRight<T> where T : IK2 {}
public interface IKind2Right<T> : IKind2, IAppLeft<T> where T : IKind2.IMu {}
public interface IKind2<TLeft, TRight> : IKind2Left<TLeft>, IKind1Right<TRight> 
    where TLeft : IK2 where TRight : IKind1.IMu {}

