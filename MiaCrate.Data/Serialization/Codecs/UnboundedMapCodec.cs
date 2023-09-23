namespace MiaCrate.Data.Codecs;

public sealed class UnboundedMapCodec<TKey, TValue> : IBaseMapCodec<TKey, TValue> where TKey : notnull
{
    public ICodec<TKey> KeyCodec { get; }

    public ICodec<TValue> ElementCodec { get; }
    
    public UnboundedMapCodec(ICodec<TKey> keyCodec, ICodec<TValue> elementCodec)
    {
        KeyCodec = keyCodec;
        ElementCodec = elementCodec;
    }

    private IBaseMapCodec<TKey, TValue> Boxed => this;

    public IDataResult<TDynamic> Encode<TDynamic>(Dictionary<TKey, TValue> input, IDynamicOps<TDynamic> ops, TDynamic prefix) => 
        Boxed.Encode(input, ops, ops.MapBuilder).Build(prefix);

    public IDataResult<IPair<Dictionary<TKey, TValue>, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input) => 
        ops.GetMap(input)
            .SetLifecycle(Lifecycle.Stable)
            .SelectMany(m => Boxed.Decode(ops, m))
            .Select(r => Pair.Of(r, input));
}