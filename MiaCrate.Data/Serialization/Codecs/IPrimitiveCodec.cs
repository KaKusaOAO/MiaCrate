namespace MiaCrate.Data.Codecs;

public interface IPrimitiveCodec : ICodec {}

public interface IPrimitiveCodec<T> : IPrimitiveCodec, ICodec<T>
{
    IDataResult<T> Read<TIn>(IDynamicOps<TIn> ops, TIn input);
    TOut Write<TOut>(IDynamicOps<TOut> ops, T value);

    IDataResult<TDynamic> IEncoder<T>.Encode<TDynamic>(T input, IDynamicOps<TDynamic> ops, TDynamic prefix) =>
        ops.MergeToPrimitive(prefix, Write(ops, input));

    IDataResult<IPair<T, TIn>> IDecoder<T>.Decode<TIn>(IDynamicOps<TIn> ops, TIn input) =>
        Read(ops, input).Select(r => Pair.Of(r, ops.Empty));
}