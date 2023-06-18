namespace MiaCrate.Data;

public interface IApplicative : IFunctor
{
    public interface IMu : IFunctor.IMu
    {
        
    }
}

public interface IApplicativeLeft<T> : IApplicative, IFunctorLeft<T> where T : IK1
{
    public IApp<T, TValue> Point<TValue>(TValue value);
    public Func<IApp<T, TArg>, IApp<T, TResult>> Lift1<TArg, TResult>(IApp<T, Func<TArg, TResult>> func);

    public IApp<T, TResult> Ap<TArg, TResult>(IApp<T, Func<TArg, TResult>> func, IApp<T, TArg> arg) =>
        Lift1(func)(arg);
    
    public IApp<T, TResult> Ap<TArg, TResult>(Func<TArg, TResult> func, IApp<T, TArg> arg) =>
        Map(func, arg);

    public IApp<T, TResult> Ap2<T1, T2, TResult>(IApp<T, Func<T1, T2, TResult>> func, IApp<T, T1> a,
        IApp<T, T2> b)
    {
        Func<T1, Func<T2, TResult>> Curry(Func<T1, T2, TResult> f) => a1 => b1 => f(a1, b1);
        return Ap(Ap(Map(Curry, func), a), b);
    }

    public IApp<T, TResult> Ap3<T1, T2, T3, TResult>(IApp<T, Func<T1, T2, T3, TResult>> func,
        IApp<T, T1> t1, IApp<T, T2> t2, IApp<T, T3> t3)
    {
        Func<T1, Func<T2, T3, TResult>> Curry(Func<T1, T2, T3, TResult> f) => a => (b, c) => f(a, b, c);
        return Ap2(Ap(Map(Curry, func), t1), t2, t3);
    }
    
    public IApp<T, TResult> Ap4<T1, T2, T3, T4, TResult>(IApp<T, Func<T1, T2, T3, T4, TResult>> func,
        IApp<T, T1> t1, IApp<T, T2> t2, IApp<T, T3> t3, IApp<T, T4> t4)
    {
        Func<T1, T2, Func<T3, T4, TResult>> Curry2(Func<T1, T2, T3, T4, TResult> f) => 
            (a, b) => (c, d) => f(a, b, c, d);
        return Ap2(Ap2(Map(Curry2, func), t1, t2), t3, t4);
    }
    
    public IApp<T, TResult> Ap5<T1, T2, T3, T4, T5, TResult>(IApp<T, Func<T1, T2, T3, T4, T5, TResult>> func,
        IApp<T, T1> t1, IApp<T, T2> t2, IApp<T, T3> t3, IApp<T, T4> t4, IApp<T, T5> t5)
    {
        Func<T1, T2, Func<T3, T4, T5, TResult>> Curry2(Func<T1, T2, T3, T4, T5, TResult> f) => 
            (a, b) => (c, d, e) => f(a, b, c, d, e);
        return Ap3(Ap2(Map(Curry2, func), t1, t2), t3, t4, t5);
    }
    
    public IApp<T, TResult> Ap6<T1, T2, T3, T4, T5, T6, TResult>(IApp<T, Func<T1, T2, T3, T4, T5, T6, TResult>> func,
        IApp<T, T1> t1, IApp<T, T2> t2, IApp<T, T3> t3, IApp<T, T4> t4, IApp<T, T5> t5, IApp<T, T6> t6)
    {
        Func<T1, T2, T3, Func<T4, T5, T6, TResult>> Curry3(Func<T1, T2, T3, T4, T5, T6, TResult> fn) => 
            (a, b, c) => (d, e, f) => fn(a, b, c, d, e, f);
        return Ap3(Ap3(Map(Curry3, func), t1, t2, t3), t4, t5, t6);
    }
}

public interface IApplicativeRight<T> : IApplicative, IFunctorRight<T> where T : IApplicative.IMu
{
}

public interface IApplicative<TLeft, TRight> : IFunctor<TLeft, TRight>, IApplicativeLeft<TLeft>, IApplicativeRight<TRight>
    where TLeft : IK1 where TRight : IApplicative.IMu
{
}