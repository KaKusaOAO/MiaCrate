using System.Runtime.CompilerServices;

namespace MiaCrate.Data.Codecs;

public interface IDecoder
{
    
}

public interface IDecoder<T> : IDecoder
{
    IDataResult<IPair<T, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input);
    IDataResult<IPair<T, TIn>> Decode<TIn>(IDynamic<TIn> input) => Decode(input.Ops, input.Value);
    IDataResult<T> Parse<TIn>(IDynamicOps<TIn> ops, TIn input) => Decode(ops, input).Select(p => p.First!);
    IDataResult<T> Parse<TIn>(IDynamic<TIn> input) => Decode(input).Select(p => p.First!);

    IDecoder<TOut> Select<TOut>(Func<T, TOut> func) => new MappedDecoder<TOut>(this, func);

    IDecoder<TOut> SelectMany<TOut>(Func<T, IDataResult<TOut>> func) => new FlatMappedDecoder<TOut>(this, func);

    IMapDecoder<T> FieldOf(string name) => new FieldDecoder<T>(name, this);

    private class MappedDecoder<TOut> : IDecoder<TOut>
    {
        private readonly IDecoder<T> _inner;
        private readonly Func<T, TOut> _converter;

        public MappedDecoder(IDecoder<T> inner, Func<T, TOut> converter)
        {
            _inner = inner;
            _converter = converter;
        }
        
        public IDataResult<IPair<TOut, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input) => 
            _inner.Decode(ops, input).Select(pair => pair.SelectFirst(_converter));
    }
    
    private class FlatMappedDecoder<TOut> : IDecoder<TOut>
    {
        private readonly IDecoder<T> _inner;
        private readonly Func<T, IDataResult<TOut>> _converter;

        public FlatMappedDecoder(IDecoder<T> inner, Func<T, IDataResult<TOut>> converter)
        {
            _inner = inner;
            _converter = converter;
        }
        
        public IDataResult<IPair<TOut, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input) => 
            _inner.Decode(ops, input).SelectMany(pair => _converter(pair.First!)
                .Select(r => Pair.Of(r, pair.Second))
            );
    }
}

public static class Decoder
{
    public static IMapDecoder<T> Unit<T>(Func<T> func) => new UnitDecoder<T>(func);
    public static IMapDecoder<T> Unit<T>(T instance) => Unit(() => instance);
    public static IDecoder<T> Error<T>(string error) => new ErrorDecoder<T>(error);
    public static WrapperBuilder<T> Wrap<T>(IDecoder<T> inner) => new(inner);

    public class WrapperBuilder<T>
    {
        private readonly IDecoder<T> _decoder;

        public WrapperBuilder(IDecoder<T> decoder)
        {
            _decoder = decoder;
        }

        public IDecoder<TOuter> With<TOuter>(Func<T, TOuter> converter) => 
            new WrappedDecoder<T, TOuter>(_decoder, converter);
    }
    
    private class WrappedDecoder<TInner, TOuter> : IDecoder<TOuter>
    {
        private readonly IDecoder<TInner> _inner;
        private readonly Func<TInner, TOuter> _converter;

        public WrappedDecoder(IDecoder<TInner> inner, Func<TInner, TOuter> converter)
        {
            _inner = inner;
            _converter = converter;
        }
        
        public IDataResult<IPair<TOuter, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input)
        {
            return _inner.Decode(ops, input).Select(pair =>
            {
                var outer = _converter(pair.First);
                return Pair.Of(outer!, pair.Second);
            });
        }
    }

    private class UnitDecoder<T> : MapDecoder.Implementation<T>
    {
        private readonly Func<T> _func;

        public UnitDecoder(Func<T> func)
        {
            _func = func;
        }

        public override IEnumerable<T1> GetKeys<T1>(IDynamicOps<T1> ops) => Enumerable.Empty<T1>();

        public override IDataResult<T> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input) => DataResult.Success(_func());
        public override string ToString() => $"UnitDecoder[{_func()}]";
    }

    private class ErrorDecoder<T> : IDecoder<T>
    {
        private readonly string _error;

        public ErrorDecoder(string error)
        {
            _error = error;
        }
        
        public IDataResult<IPair<T, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input) => 
            DataResult.Error<IPair<T, TIn>>(() => _error);

        public override string ToString() => $"ErrorDecoder[{_error}]";
    }
}

