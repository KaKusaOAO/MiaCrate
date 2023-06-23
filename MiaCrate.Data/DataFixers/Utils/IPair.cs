namespace MiaCrate.Data;

public interface IPair : IApp {}
public interface IPairFirst<T> : IPair, IAppRight<T> {}
public interface IPairSecond<T> : IPair, IAppLeft<IPairSecond<T>.Mu>
{
    public class Mu : IK1 {}
}

public interface IPair<TFirst, TSecond> : IPairFirst<TFirst>, IPairSecond<TSecond>, IApp<IPairSecond<TSecond>.Mu, TFirst>
{
    public TFirst First { get; }
    public TSecond Second { get; }
    public IPair<TSecond, TFirst> Swap() => Pair.Of(Second, First);
    public IPair<TOut, TSecond> SelectFirst<TOut>(Func<TFirst, TOut> func) => Pair.Of(func(First), Second);
}

internal class Pair<TFirst, TSecond> : IPair<TFirst, TSecond>
{
    public TFirst First { get; }
    public TSecond Second { get; }

    public Pair(TFirst first, TSecond second)
    {
        First = first;
        Second = second;
    }
}

public static class Pair
{
    public static IPair<TFirst, TSecond> Of<TFirst, TSecond>(TFirst first, TSecond second) => 
        new Pair<TFirst, TSecond>(first, second);
    
    
}

