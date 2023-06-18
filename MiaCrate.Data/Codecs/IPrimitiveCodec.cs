namespace MiaCrate.Data.Codecs;

public interface IPrimitiveCodec : ICodec {}

public interface IPrimitiveCodec<T> : IPrimitiveCodec, ICodec<T>
{
    IDataResult<T> Read<TIn>(IDynamicOps<TIn> ops, TIn input);
    TOut Write<TOut>(IDynamicOps<TOut> ops, T value);
}