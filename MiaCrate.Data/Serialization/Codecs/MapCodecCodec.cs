namespace MiaCrate.Data.Codecs;

public interface IMapCodecCodec : ICodec
{
    public IMapCodec Codec { get; }
}

public interface IMapCodecCodec<T> : IMapCodecCodec, ICodec<T>
{
    public new IMapCodec<T> Codec { get; }
    IMapCodec IMapCodecCodec.Codec => Codec;
    
    ICodec<TOut> ICodec<T>.Cast<TOut>() => new Casted<TOut>(this);

    private class Casted<TOut> : IMapCodecCodec<TOut>
    {
        private readonly IMapCodecCodec<T> _inner;

        public Casted(IMapCodecCodec<T> inner)
        {
            _inner = inner;
        }

        public IDataResult<TDynamic> Encode<TDynamic>(TOut input, IDynamicOps<TDynamic> ops, TDynamic prefix) => 
            _inner.Encode((T) (object) input!, ops, prefix);

        public IDataResult<IPair<TOut, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input) => 
            _inner.Decode(ops, input).Select(p => Pair.Of((TOut) (object) p.First!, p.Second!));

        public IMapCodec<TOut> Codec => _inner.Codec.Cast<TOut>();
    }
}

public class MapCodecCodec<T> : IMapCodecCodec<T>
{
    public IMapCodec<T> Codec { get; }
    
    public MapCodecCodec(IMapCodec<T> codec)
    {
        Codec = codec;
    }

    public IDataResult<IPair<T, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input) => 
        Codec.CompressedDecode(ops, input).Select(r => Pair.Of(r, input));

    public IDataResult<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, TOut prefix) =>
        Codec.Encode(input, ops, Codec.GetCompressedBuilder(ops)).Build(prefix);
}