using MiaCrate.Data.Utils;

namespace MiaCrate.Data.Codecs;

public interface IDecoder
{
    
}

public interface IDecoder<T> : IDecoder
{
    IDataResult<IPair<T, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input);
    IDataResult<T> Parse<TIn>(IDynamicOps<TIn> ops, TIn input) => Decode(ops, input).Select(p => p.First);
}