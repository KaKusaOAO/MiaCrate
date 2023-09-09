using Mochi.Utils;

namespace MiaCrate.Data.Codecs;

public class OptionalFieldCodec<T> : MapCodec<IOptional<T>>
{
    private readonly string _name;
    private readonly ICodec<T> _elementCodec;

    public OptionalFieldCodec(string name, ICodec<T> elementCodec)
    {
        _name = name;
        _elementCodec = elementCodec;
    }

    public override IDataResult<IOptional<T>> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input)
    {
        var value = input[_name];
        if (value == null) return DataResult.Success(Optional.Empty<T>());

        var parsed = _elementCodec.Parse(ops, value);
        return parsed.Result.IsPresent 
            ? parsed.Select(Optional.Of) 
            : DataResult.Success(Optional.Empty<T>());
    }

    public override IRecordBuilder<TOut> Encode<TOut>(IOptional<T> input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix) => 
        input.IsPresent 
            ? prefix.Add(_name, _elementCodec.EncodeStart(ops, input.Value)) 
            : prefix;

    public override IEnumerable<TOut> GetKeys<TOut>(IDynamicOps<TOut> ops) => new[] {ops.CreateString(_name)};

    public override string ToString() => $"OptionalFieldCodec[{_name}: {_elementCodec}]";
}