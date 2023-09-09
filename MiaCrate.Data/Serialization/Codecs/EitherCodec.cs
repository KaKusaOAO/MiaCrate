namespace MiaCrate.Data.Codecs;

public class EitherCodec<TFirst, TSecond> : ICodec<IEither<TFirst, TSecond>>
{
    private readonly ICodec<TFirst> _first;
    private readonly ICodec<TSecond> _second;

    public EitherCodec(ICodec<TFirst> first, ICodec<TSecond> second)
    {
        _first = first;
        _second = second;
    }

    public IDataResult<TDynamic> Encode<TDynamic>(IEither<TFirst, TSecond> input, IDynamicOps<TDynamic> ops, TDynamic prefix)
    {
        return input.Select(
            first => _first.Encode(first, ops, prefix),
            second => _second.Encode(second, ops, prefix)
        );
    }

    public IDataResult<IPair<IEither<TFirst, TSecond>, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input)
    {
        var firstRead = _first.Decode(ops, input).Select(v => v.SelectFirst(Either.Left<TFirst, TSecond>));
        return firstRead.Result.IsPresent 
            ? firstRead 
            : _second.Decode(ops, input).Select(v => v.SelectFirst(Either.Right<TFirst, TSecond>));
    }

    public override string ToString() => $"EitherCodec[{_first}, {_second}]";
}