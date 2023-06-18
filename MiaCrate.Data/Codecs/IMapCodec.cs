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

public class MapCodecCodec<T> : ICodec<T>
{
    public IMapCodec<T> Codec { get; }

    public MapCodecCodec(IMapCodec<T> codec)
    {
        Codec = codec;
    }
}

public class MapCodec<T> : IMapCodec<T>
{
    public ICodec<T> Codec => new MapCodecCodec<T>(this);
    public IRecordCodecBuilder<TLeft, T> ForGetter<TLeft>(Func<TLeft, T> getter) => RecordCodecBuilder.Of(getter, this);
}