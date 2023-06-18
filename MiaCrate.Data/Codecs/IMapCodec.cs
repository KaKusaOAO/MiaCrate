namespace MiaCrate.Data.Codecs;

public interface IMapCodec : IMapEncoder, IMapDecoder
{
    ICodec Codec { get; }
}

public interface IMapCodec<T> : IMapCodec, IMapEncoder<T>, IMapDecoder<T>
{
    new ICodec<T> Codec { get; }
    ICodec IMapCodec.Codec => Codec;

    IRecordCodecBuilder<TLeft, T> ForGetter<TLeft>(Func<TLeft, T> getter);
}

public abstract class MapCodec<T> : CompressorHolder, IMapCodec<T>
{
    public ICodec<T> Codec => new MapCodecCodec<T>(this);
    public IRecordCodecBuilder<TLeft, T> ForGetter<TLeft>(Func<TLeft, T> getter) => RecordCodecBuilder.Of(getter, this);
    public abstract IRecordBuilder<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix);
    public abstract IDataResult<T> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input);
}

internal class BasicNamedMapCodec<T> : MapCodec<T>
{
    private readonly IMapEncoder<T> _encoder;
    private readonly IMapDecoder<T> _decoder;
    private readonly Func<string> _nameFunc;

    public BasicNamedMapCodec(IMapEncoder<T> encoder, IMapDecoder<T> decoder, Func<string> nameFunc)
    {
        _encoder = encoder;
        _decoder = decoder;
        _nameFunc = nameFunc;
    }

    public override IEnumerable<T1> GetKeys<T1>(IDynamicOps<T1> ops) => 
        _encoder.GetKeys(ops).Concat(_decoder.GetKeys(ops));
    
    public override IDataResult<T> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input) => 
        _decoder.Decode(ops, input);

    public override IRecordBuilder<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix) =>
        _encoder.Encode(input, ops, prefix);
}