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
}

public static class MapEncoder
{
    public static IRecordBuilder<T> MakeCompressedBuilder<T>(IDynamicOps<T> ops, IKeyCompressor<T> compressor) => 
        new CompressedRecordBuilder<T>(ops, compressor);

    internal abstract class Implementation<T> : CompressorHolder, IMapEncoder<T>
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