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

public static class Decoder
{
    public static IMapDecoder<T> Unit<T>(Func<T> func) => new UnitDecoder<T>(func);
    public static IMapDecoder<T> Unit<T>(T instance) => Unit(() => instance);
}

internal class UnitDecoder<T> : IMapDecoder<T>
{
    private readonly Func<T> _func;

    public UnitDecoder(Func<T> func)
    {
        _func = func;
    }

    public IEnumerable<T1> GetKeys<T1>(IDynamicOps<T1> ops) => Enumerable.Empty<T1>();

    public IDataResult<T> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input) => DataResult.Success(_func());
}