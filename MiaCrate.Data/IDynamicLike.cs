using System.Numerics;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Data;

public interface IDynamicLike
{
    IDynamicOps Ops { get; }

    IDataResult<TNumber> AsNumber<TNumber>() where TNumber : INumber<TNumber>;
    IDataResult<string> AsString();
}

public interface IDynamicLike<T> : IDynamicLike
{
    new IDynamicOps<T> Ops { get; }
    IDynamicOps IDynamicLike.Ops => Ops;
    
    IDataResult<IEnumerable<IDynamic<T>>> AsEnumerable();
}