namespace MiaCrate.Data.Codecs;

public interface ICodec : IEncoder, IDecoder
{
    
}

public interface ICodec<T> : ICodec, IEncoder<T>, IDecoder<T>
{
    
}