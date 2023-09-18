using System.Runtime.CompilerServices;

namespace MiaCrate.Data.Codecs;

public interface IEncoder
{
    
}

public interface IEncoder<T> : IEncoder
{
    IDataResult<TDynamic> Encode<TDynamic>(T input, IDynamicOps<TDynamic> ops, TDynamic prefix);

    // Why the arguments are reversed?
    IDataResult<TDynamic> EncodeStart<TDynamic>(IDynamicOps<TDynamic> ops, T input) =>
        Encode(input, ops, ops.Empty);

    IEncoder<TIn> CoSelect<TIn>(Func<TIn, T> func) => new CoMappedEncoder<TIn>(this, func);

    IEncoder<TIn> FlatCoSelect<TIn>(Func<TIn, IDataResult<T>> func) => new FlatCoMappedEncoder<TIn>(this, func);

    IMapEncoder<T> FieldOf(string name) => new FieldEncoder<T>(name, this);

    private class CoMappedEncoder<TOuter> : IEncoder<TOuter>
    {
        private readonly IEncoder<T> _inner;
        private readonly Func<TOuter, T> _converter;

        public CoMappedEncoder(IEncoder<T> inner, Func<TOuter, T> converter)
        {
            _inner = inner;
            _converter = converter;
        }

        public IDataResult<TDynamic> Encode<TDynamic>(TOuter input, IDynamicOps<TDynamic> ops, TDynamic prefix)
        {
            var inner = _converter(input);
            return _inner.Encode(inner!, ops, prefix);
        }

        public override string ToString() => _inner + "[comapped]";
    }

    private class FlatCoMappedEncoder<TOuter> : IEncoder<TOuter>
    {
        private readonly IEncoder<T> _inner;
        private readonly Func<TOuter, IDataResult<T>> _converter;
        
        public FlatCoMappedEncoder(IEncoder<T> encoder, Func<TOuter, IDataResult<T>> func)
        {
            _inner = encoder;
            _converter = func;
        }

        public IDataResult<TDynamic> Encode<TDynamic>(TOuter input, IDynamicOps<TDynamic> ops, TDynamic prefix) => 
            _converter(input).SelectMany(a => _inner.Encode(a, ops, prefix));

        public override string ToString() => _inner + "[flatComapped]";
    }
}

public static class Encoder
{
    public static IMapEncoder<T> Empty<T>() => new EmptyMapEncoder<T>();
    public static IEncoder<T> Error<T>(string error) => new ErrorEncoder<T>(error);
    
    private class EmptyMapEncoder<T> : MapEncoder.Implementation<T>
    {
        public override IEnumerable<T1> GetKeys<T1>(IDynamicOps<T1> ops) => Enumerable.Empty<T1>();
        public override IRecordBuilder<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix) => prefix;
    }

    private class ErrorEncoder<T> : IEncoder<T>
    {
        private readonly string _error;

        public ErrorEncoder(string error)
        {
            _error = error;
        }
        
        public IDataResult<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, TOut prefix) => 
            DataResult.Error<TOut>(() => $"{_error} {input}");

        public override string ToString() => $"ErrorEncoder[{_error}]";
    }
}