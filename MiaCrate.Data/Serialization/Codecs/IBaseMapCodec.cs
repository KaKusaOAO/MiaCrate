using Mochi.Core;
using Mochi.Utils;

namespace MiaCrate.Data.Codecs;

public interface IBaseMapCodec<TKey, TValue> : ICodec<Dictionary<TKey, TValue>> where TKey : notnull
{
    ICodec<TKey> KeyCodec { get; }
    ICodec<TValue> ElementCodec { get; }

    IDataResult<Dictionary<TKey, TValue>> Decode<T>(IDynamicOps<T> ops, IMapLike<T> input)
    {
        var read = new Dictionary<TKey, TValue>();
        var failed = new List<IPair<T, T>>();

        // combiner.apply(u, accumulator.apply(identity, t)) == accumulator.apply(u, t)
        var result = input.Entries.Aggregate(
            DataResult.Success(Unit.Instance, Lifecycle.Stable),
            (r, pair) =>
            {
                var k = KeyCodec.Parse(ops, pair.First!);
                var v = ElementCodec.Parse(ops, pair.Second!);
                var entry = k.Apply2Stable(Pair.Of, v);
                entry.Error.IfPresent(e => failed.Add(pair));

                return r.Apply2Stable((u, p) =>
                {
                    read[p.First!] = p.Second!;
                    return u;
                }, entry);
            });

        var elements = read;
        var errors = ops.CreateMap(failed);

        return result
            .Select(_ => elements)
            .SetPartial(elements)
            .SelectError(e => e + " missed input: " + errors);
    }

    IRecordBuilder<T> Encode<T>(Dictionary<TKey, TValue> input, IDynamicOps<T> ops, IRecordBuilder<T> prefix)
    {
        foreach (var (key, value) in input)
        {
            prefix.Add(KeyCodec.EncodeStart(ops, key), ElementCodec.EncodeStart(ops, value));
        }

        return prefix;
    }
}