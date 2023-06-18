namespace MiaCrate.Data;

public interface IIdF : IAppLeft<IIdF.Mu>
{
    public sealed class Mu : IK1 {}
    public static IIdF<T> Create<T>(T value) => new IdF<T>(value);
    public static T Get<T>(IApp<Mu, T> box) => ((IIdF<T>)box).Value;
}

public interface IIdF<T> : IApp<IIdF.Mu, T>
{
    public T Value { get; }
}

internal class IdF<T> : IIdF<T>
{
    public IdF(T value)
    {
        Value = value;
    }

    public T Value { get; }
}