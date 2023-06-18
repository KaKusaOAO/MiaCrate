namespace MiaCrate.Data.Codecs;

public interface IEncoder
{
    
}

public interface IEncoder<T> : IEncoder
{
    IDataResult<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, TOut prefix);

    IDataResult<TOut> EncodeStart<TOut>(T input, IDynamicOps<TOut> ops) =>
        Encode(input, ops, ops.Empty);
}