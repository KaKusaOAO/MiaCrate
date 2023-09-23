using System.Collections;

namespace MiaCrate.Data.Codecs;

public interface IMapDecoder : ICompressable
{
    IDataResult Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input);
    IDataResult CompressedDecode<TIn>(IDynamicOps<TIn> ops, TIn input);
}

public interface IMapDecoder<T> : IMapDecoder
{
    new IDataResult<T> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input);
    IDataResult IMapDecoder.Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input) => Decode(ops, input);

    IMapDecoder<TOut> Select<TOut>(Func<T, TOut> func) => 
        new IMapDecoder<TOut>.MappedImpl<T>(this, func);
    IMapDecoder<TOut> SelectMany<TOut>(Func<T, IDataResult<TOut>> func) => 
        new IMapDecoder<TOut>.FlatMappedImpl<T>(this, func);

    new IDataResult<T> CompressedDecode<TIn>(IDynamicOps<TIn> ops, TIn input)
    {
        if (!ops.CompressMaps)
        {
            return ops
                .GetMap(input)
                .SetLifecycle(Lifecycle.Stable)
                .SelectMany(m => Decode(ops, m));
        }

        var inputList = ops.GetList(input).Result;
        if (!inputList.IsPresent)
            return DataResult.Error<T>(() => "Input is not a list");

        var compressor = GetCompressor(ops);
        var entries = new List<TIn>();
        inputList.Value(e => entries.Add(e));

        var map = new CompressedMap<TIn>(compressor, entries);
        return Decode(ops, map);
    }

    IDataResult IMapDecoder.CompressedDecode<TIn>(IDynamicOps<TIn> ops, TIn input) =>
        CompressedDecode(ops, input);

    private class CompressedMap<TOps> : IMapLike<TOps>
    {
        private readonly IKeyCompressor<TOps> _compressor;
        private readonly List<TOps> _entries;

        public CompressedMap(IKeyCompressor<TOps> compressor, List<TOps> entries)
        {
            _compressor = compressor;
            _entries = entries;
        }

        public TOps? this[TOps key] => _entries[_compressor.Compress(key)];
        
        public TOps? this[string key] => _entries[_compressor.Compress(key)];

        public IEnumerable<IPair<TOps, TOps>> Entries =>
            Enumerable.Range(0, _entries.Count)
                .Select(i => Pair.Of(_compressor.Decompress(i), _entries[i]))
                .Where(p => p.Second != null);
    }

    private class FlatMappedImpl<TSource> : MapDecoder.Implementation<T>
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
    
    private class MappedImpl<TSource> : MapDecoder.Implementation<T>
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

public static class MapDecoder
{
    public abstract class Implementation<T> : CompressorHolder, IMapDecoder<T>
    {
        public abstract IDataResult<T> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input);
    }
}