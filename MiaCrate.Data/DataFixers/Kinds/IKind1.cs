namespace MiaCrate.Data;

public interface IKind1
{
    public interface IMu : IK1
    {
        
    }
}

public interface IKind1Left<T> : IKind1, IAppRight<T> where T : IK1
{
}

public interface IKind1Right<T> : IKind1, IAppLeft<T> where T : IKind1.IMu
{
}

public interface IKind1<TLeft, TRight> : IKind1Left<TLeft>, IKind1Right<TRight>, IApp<TRight, TLeft> 
    where TLeft : IK1 where TRight : IKind1.IMu
{
}

public static class Kind1Extension
{
    public static Products.IP1<TLeft, T1> Group<TLeft, T1>
        (this IKind1Left<TLeft> kind, IApp<TLeft, T1> t1) 
        where TLeft : IK1 => 
        new Products.P1<TLeft, T1>(t1);
    
    public static Products.IP2<TLeft, T1, T2> Group<TLeft, T1, T2>
        (this IKind1Left<TLeft> kind, IApp<TLeft, T1> t1, IApp<TLeft, T2> t2) 
        where TLeft : IK1 => 
        new Products.P2<TLeft, T1, T2>(t1, t2);
    
    public static Products.IP3<TLeft, T1, T2, T3> Group<TLeft, T1, T2, T3>
        (this IKind1Left<TLeft> kind, IApp<TLeft, T1> t1, IApp<TLeft, T2> t2, IApp<TLeft, T3> t3) 
        where TLeft : IK1 => 
        new Products.P3<TLeft, T1, T2, T3>(t1, t2, t3);
    
    public static Products.IP4<TLeft, T1, T2, T3, T4> Group<TLeft, T1, T2, T3, T4>
        (this IKind1Left<TLeft> kind, IApp<TLeft, T1> t1, IApp<TLeft, T2> t2, IApp<TLeft, T3> t3, IApp<TLeft, T4> t4) 
        where TLeft : IK1 => 
        new Products.P4<TLeft, T1, T2, T3, T4>(t1, t2, t3, t4);
}