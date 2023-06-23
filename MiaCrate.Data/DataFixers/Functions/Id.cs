namespace MiaCrate.Data;

public interface IId : IFuncPointFree {}

public interface IId<T> : IId, IFuncPointFree<T, T>
{
    public new IType<IFunction<T, T>> Type { get; }
    IType IPointFree.Type => Type;
}

public sealed class Id<T> : PointFree<IFunction<T, T>>, IId<T>
{
    public override IType<IFunction<T, T>> Type { get; }

    public Id(IType<IFunction<T, T>> type)
    {
        Type = type;
    }
    
    public override Func<IDynamicOps, IFunction<T, T>> Eval() => _ => Function.Create<T, T>(t => t);
    public override string ToString(int level) => "id";

    public override int GetHashCode() => Type.GetHashCode();

    public override bool Equals(object? obj) => obj is Id<T> id && Type.Equals(id.Type);

    public static bool operator ==(Id<T> a, Id<T> b) => a.Equals(b);

    public static bool operator !=(Id<T> a, Id<T> b) => !(a == b);
}