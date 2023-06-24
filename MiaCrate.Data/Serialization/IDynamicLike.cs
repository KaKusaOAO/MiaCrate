using System.Numerics;
using MiaCrate.Data.Codecs;
using Mochi.Utils;

namespace MiaCrate.Data;

public interface IDynamicLike
{
    IDynamicOps Ops { get; }

    IDataResult<decimal> AsNumber();
    decimal AsNumber(decimal def);
    int AsInt(int def);
    double AsDouble(double def);
    
    IDataResult<string> AsString();
    string AsString(string def);
}

public interface IDynamicLike<T> : IDynamicLike
{
    new IDynamicOps<T> Ops { get; }
    IDynamicOps IDynamicLike.Ops => Ops;
    
    IEnumerable<IDynamic<T>> AsEnumerable();
    
    IDataResult<IEnumerable<IDynamic<T>>> AsEnumerableOpt();
}

public abstract class DynamicLike<T> : IDynamicLike<T>
{
    public IDynamicOps<T> Ops { get; }

    public DynamicLike(IDynamicOps<T> ops)
    {
        Ops = ops;
    }
    
    public abstract IDataResult<decimal> AsNumber();
    public abstract IDataResult<string> AsString();
    public abstract IDataResult<IEnumerable<IDynamic<T>>> AsEnumerableOpt();

    public decimal AsNumber(decimal def) => AsNumber().Result.OrElse(def);
    public int AsInt(int def) => (int)AsNumber(def);
    public double AsDouble(double def) => (double)AsNumber((decimal)def);
    public string AsString(string def) => AsString().Result.OrElse(def);
    public IEnumerable<IDynamic<T>> AsEnumerable() => 
        AsEnumerableOpt().Result.OrElse(Enumerable.Empty<IDynamic<T>>);
}