namespace MiaCrate.Data.Codecs;

public interface IEncoder
{
    
}

public interface IEncoder<T> : IEncoder
{
    IDataResult<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, TOut prefix);

    // Why the arguments are reversed?
    IDataResult<TOut> EncodeStart<TOut>(IDynamicOps<TOut> ops, T input) =>
        Encode(input, ops, ops.Empty);
}

public static class Encoder
{
    public static IMapEncoder<T> Empty<T>() => new EmptyMapEncoder<T>();
}

internal class EmptyMapEncoder<T> : MapEncoder.Implementation<T>
{
    public override IEnumerable<T1> GetKeys<T1>(IDynamicOps<T1> ops) => Enumerable.Empty<T1>();
    public override IRecordBuilder<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix) => prefix;

}