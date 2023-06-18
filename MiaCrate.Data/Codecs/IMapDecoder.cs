using System.Collections;

namespace MiaCrate.Data.Codecs;

public interface IMapDecoder : IKeyable
{
    IDataResult Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input);
}

public interface IMapDecoder<T> : IMapDecoder
{
    new IDataResult<T> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input);
    IDataResult IMapDecoder.Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input) => Decode(ops, input);

    IMapDecoder<TOut> Select<TOut>(Func<T, TOut> func) => 
        new IMapDecoder<TOut>.MappedImpl<T>(this, func);
    IMapDecoder<TOut> SelectMany<TOut>(Func<T, IDataResult<TOut>> func) => 
        new IMapDecoder<TOut>.FlatMappedImpl<T>(this, func);

    public abstract class Impl : CompressorHolder, IMapDecoder<T>
    {
        public abstract IDataResult<T> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input);
    }

    private class FlatMappedImpl<TSource> : Impl
    {
        private IMapDecoder<TSource> _decoder;
        private readonly Func<TSource, IDataResult<T>> _func;

        public FlatMappedImpl(IMapDecoder<TSource> decoder, Func<TSource, IDataResult<T>> func)
        {
            _decoder = decoder;
            _func = func;
        }

        public override IEnumerable<T1> GetKeys<T1>(IDynamicOps<T1> ops) => _decoder.GetKeys(ops);
        public override IDataResult<T> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input) => 
            _decoder.Decode(ops, input).SelectMany(_func);

        public override string ToString() => _decoder + "[flatMapped]";
    }
    
    private class MappedImpl<TSource> : Impl
    {
        private IMapDecoder<TSource> _decoder;
        private readonly Func<TSource, T> _func;

        public MappedImpl(IMapDecoder<TSource> decoder, Func<TSource, T> func)
        {
            _decoder = decoder;
            _func = func;
        }

        public override IEnumerable<T1> GetKeys<T1>(IDynamicOps<T1> ops) => _decoder.GetKeys(ops);
        public override IDataResult<T> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input) => 
            _decoder.Decode(ops, input).Select(_func);

        public override string ToString() => _decoder + "[mapped]";
    }
}