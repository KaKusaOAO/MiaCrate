using Mochi.Core;
using Mochi.Utils;

namespace MiaCrate.Data.Codecs;

public sealed class ListCodec<T> : ICodec<List<T>>
{
    private readonly ICodec<T> _elementCodec;

    public ListCodec(ICodec<T> elementCodec)
    {
        _elementCodec = elementCodec;
    }

    public IDataResult<TDynamic> Encode<TDynamic>(List<T> input, IDynamicOps<TDynamic> ops, TDynamic prefix)
    {
        var builder = ops.ListBuilder;
        foreach (var a in input)
        {
            builder.Add(_elementCodec.EncodeStart(ops, a));
        }
        return builder.Build(prefix);
    }

    public IDataResult<IPair<List<T>, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input)
    {
        return ops.GetList(input).SetLifecycle(Lifecycle.Stable).SelectMany(stream =>
        {
            var read = new List<T>();
            var failed = new List<TIn>();
            var result = DataResult.Success(Unit.Instance, Lifecycle.Stable);

            stream(t =>
            {
                var element = _elementCodec.Decode(ops, t);
                element.Error.IfPresent(_ => failed.Add(t));
                result = result.Apply2Stable((r, v) =>
                {
                    read.Add(v.First!);
                    return r;
                }, element);
            });

            var errors = ops.CreateList(failed);
            var pair = Pair.Of(read, errors);
            return result.Select(_ => pair).SetPartial(pair);
        });
    }

    public override string ToString() => $"ListCodec[{_elementCodec}]";
}