namespace MiaCrate.Data.Codecs;

public interface IMapCodecCodec : ICodec
{
    public IMapCodec Codec { get; }
}

public interface IMapCodecCodec<T> : IMapCodecCodec, ICodec<T>
{
    public new IMapCodec<T> Codec { get; }
    IMapCodec IMapCodecCodec.Codec => Codec;
}

public class MapCodecCodec<T> : IMapCodecCodec<T>
{
    public IMapCodec<T> Codec { get; }
    
    public MapCodecCodec(IMapCodec<T> codec)
    {
        Codec = codec;
    }

    public IDataResult<IPair<T, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input)
    {
        throw new NotImplementedException();
    }

    public IDataResult<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, TOut prefix) =>
        Codec.Encode(input, ops, Codec.GetCompressedBuilder(ops)).Build(prefix);
}