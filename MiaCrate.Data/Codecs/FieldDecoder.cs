namespace MiaCrate.Data.Codecs;

public sealed class FieldDecoder<T> : IMapDecoder<T>.Impl
{
    private readonly string _name;
    private readonly IDecoder<T> _elementCodec;

    public FieldDecoder(string name, IDecoder<T> elementCodec)
    {
        _name = name;
        _elementCodec = elementCodec;
    }

    public override IEnumerable<T1> Keys<T1>(IDynamicOps<T1> ops) => new[] { ops.CreateString(_name) };

    public override IDataResult<T> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input)
    {
        var value = input[_name];
        if (value == null) return DataResult.Error<T>(() => $"No key {_name} in {input}");
        return _elementCodec.Parse(ops, value);
    }
}