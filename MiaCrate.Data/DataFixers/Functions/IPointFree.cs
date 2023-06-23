using Mochi.Utils;

namespace MiaCrate.Data;

public interface IPointFree
{
    public IType Type { get; }
    public Func<IDynamicOps, object> EvalCached();
    public string ToString(int level);
}

public interface IPointFree<T> : IPointFree
{
    public new Func<IDynamicOps, T> EvalCached();
    Func<IDynamicOps, object> IPointFree.EvalCached()
    {
        var func = EvalCached();
        return ops => func(ops)!;
    }

    public new IType<T> Type { get; }
    IType IPointFree.Type => Type;
    
    public Func<IDynamicOps, T> Eval();
    public IOptional<IPointFree<T>> All(IPointFreeRule rule);
    public IOptional<IPointFree<T>> One(IPointFreeRule rule);
}

public abstract class PointFree<T> : IPointFree<T>
{
    private bool _initialized;
    private Func<IDynamicOps, T>? _value;
    private readonly SemaphoreSlim _lock = new(1, 1);
    
    public Func<IDynamicOps, T> EvalCached()
    {
        if (_initialized) return _value!;
        _lock.Wait();

        if (_initialized) return _value!;
        _value = Eval();
        _initialized = true;

        _lock.Release();
        return _value;
    }

    public abstract IType<T> Type { get; }
    public abstract Func<IDynamicOps, T> Eval();

    public virtual IOptional<IPointFree<T>> All(IPointFreeRule rule) => Optional.Of<IPointFree<T>>(this);
    public virtual IOptional<IPointFree<T>> One(IPointFreeRule rule) => Optional.Empty<IPointFree<T>>();

    protected static string Indent(int level) => "  ".Repeat(level);
    public abstract string ToString(int level);
}