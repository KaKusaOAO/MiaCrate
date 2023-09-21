namespace MiaCrate.Data.Codecs;

public sealed class PairMapCodec<TFirst, TSecond> : MapCodec<IPair<TFirst, TSecond>>
{
    private readonly IMapCodec<TFirst> _first;
    private readonly IMapCodec<TSecond> _second;
    
    public PairMapCodec(IMapCodec<TFirst> first, IMapCodec<TSecond> second)
    {
        _first = first;
        _second = second;
    }

    public override IEnumerable<T> GetKeys<T>(IDynamicOps<T> ops) => _first.GetKeys(ops).Concat(_second.GetKeys(ops));

    public override IRecordBuilder<TOut> Encode<TOut>(IPair<TFirst, TSecond> input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix) => 
        _first.Encode(input.First!, ops, _second.Encode(input.Second!, ops, prefix));

    public override IDataResult<IPair<TFirst, TSecond>> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input)
    {
        return _first.Decode(ops, input).SelectMany(p =>
            _second.Decode(ops, input).Select(p2 =>
                Pair.Of(p, p2)));
    }

    public override string ToString() => $"PairMapCodec[{_first}, {_second}]";
}