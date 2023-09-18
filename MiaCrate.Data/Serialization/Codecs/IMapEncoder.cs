using Mochi.Utils;

namespace MiaCrate.Data.Codecs;

public interface IMapEncoder : IKeyable
{
    
}

public interface IMapEncoder<T> : IMapEncoder
{
    IRecordBuilder<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix);
    IKeyCompressor<TOut> GetCompressor<TOut>(IDynamicOps<TOut> ops);

    IRecordBuilder<TOut> GetCompressedBuilder<TOut>(IDynamicOps<TOut> ops) => ops.CompressMaps
        ? MapEncoder.MakeCompressedBuilder(ops, GetCompressor(ops))
        : ops.MapBuilder;

    public IMapEncoder<TOut> CoSelect<TOut>(Func<TOut, T> func) => new CoMappedEncoder<TOut>(this, func);

    public IMapEncoder<TOut> FlatCoSelect<TOut>(Func<TOut, IDataResult<T>> func) =>
        new FlatCoMappedEncoder<TOut>(this, func); 

    private class FlatCoMappedEncoder<TOuter> : MapEncoder.Implementation<TOuter>
    {
        private readonly IMapEncoder<T> _inner;
        private readonly Func<TOuter, IDataResult<T>> _converter;

        public FlatCoMappedEncoder(IMapEncoder<T> inner, Func<TOuter, IDataResult<T>> converter)
        {
            _inner = inner;
            _converter = converter;
        }

        public override IEnumerable<TKey> GetKeys<TKey>(IDynamicOps<TKey> ops) => _inner.GetKeys(ops);

        public override IRecordBuilder<TOut> Encode<TOut>(TOuter input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix)
        {
            var result = _converter(input);
            var builder = prefix.WithErrorsFrom(result);
            return result.Select(r => _inner.Encode(r, ops, builder)).Result.OrElse(builder);
        }

        public override string ToString() => $"{this}[flatComapped]";
    }
    
    private class CoMappedEncoder<TOuter> : MapEncoder.Implementation<TOuter>
    {
        private readonly IMapEncoder<T> _inner;
        private readonly Func<TOuter, T> _converter;

        public CoMappedEncoder(IMapEncoder<T> inner, Func<TOuter, T> converter)
        {
            _inner = inner;
            _converter = converter;
        }

        public override IEnumerable<TKey> GetKeys<TKey>(IDynamicOps<TKey> ops) => _inner.GetKeys(ops);

        public override IRecordBuilder<TOut> Encode<TOut>(TOuter input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix) => 
            _inner.Encode(_converter(input), ops, prefix);

        public override string ToString() => $"{this}[comapped]";
    }
}

public static class MapEncoder
{
    public static IRecordBuilder<T> MakeCompressedBuilder<T>(IDynamicOps<T> ops, IKeyCompressor<T> compressor) => 
        new CompressedRecordBuilder<T>(ops, compressor);

    public abstract class Implementation<T> : CompressorHolder, IMapEncoder<T>
    {
        public abstract IRecordBuilder<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix);
    }
}

internal class CompressedRecordBuilder<T> : AbstractUniversalBuilder<T, List<T>>
{
    private readonly IKeyCompressor<T> _compressor;

    public CompressedRecordBuilder(IDynamicOps<T> ops, IKeyCompressor<T> compressor, Func<List<T>>? builder = null) : base(ops, builder)
    {
        _compressor = compressor;
    }

    protected override IDataResult<T> Build(List<T> builder, T prefix) => Ops.MergeToList(prefix, builder);

    protected override List<T> InitBuilder()
    {
        var list = new List<T>();
        for (var i = 0; i < _compressor.Size; i++)
        {
            list.Add(default!);
        }

        return list;
    }

    protected override List<T> Append(T key, T value, List<T> builder)
    {
        builder[_compressor.Compress(key)] = value;
        return builder;
    }
}