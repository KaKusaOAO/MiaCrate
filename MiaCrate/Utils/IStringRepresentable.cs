using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Extensions;

namespace MiaCrate;

public interface IStringRepresentable
{
    public string SerializedName { get; }

    public static EnumCodec<T> FromEnum<T>(Func<T[]> values) where T : class, IStringRepresentable, IEnumLike<T> =>
        FromEnumWithMapping(values, s => s);
    
    public static EnumCodec<T> FromEnumWithMapping<T>(Func<T[]> values, Func<string, string> mapper) where T : class, IStringRepresentable, IEnumLike<T>
    {
        var arr = values();
        if (arr.Length > 16)
        {
            var dict = arr.ToDictionarySkipDuplicates(
                t => mapper(t.SerializedName),
                t => t);
            return new EnumCodec<T>(arr, str => str == null ? null : dict.GetValueOrDefault(str));
        }

        return new EnumCodec<T>(arr, str =>
        {
            foreach (var item in arr)
            {
                if (mapper(item.SerializedName) == str) return item;
            }

            return null;
        });
    }

    public class EnumCodec<T> : ICodec<T> where T : class, IStringRepresentable, IEnumLike<T>
    {
        private readonly ICodec<T> _codec;
        private readonly Func<string?, T?> _resolver;

        public EnumCodec(T[] values, Func<string?, T?> resolver)
        {
            _codec = ExtraCodecs.OrCompressed(
                ExtraCodecs.StringResolverCodec(obj => obj.SerializedName, resolver),
                ExtraCodecs.IdResolverCodec(
                    obj => obj.Ordinal,
                    i => i >= 0 && i < values.Length ? values[i] : null, -1)
            );
            _resolver = resolver;
        }

        public IDataResult<TDynamic> Encode<TDynamic>(T input, IDynamicOps<TDynamic> ops, TDynamic prefix) => 
            _codec.Encode(input, ops, prefix);

        public IDataResult<IPair<T, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input) =>
            _codec.Decode(ops, input);

        public T? ByName(string? name) => _resolver(name);
        public T ByName(string? name, T defaultValue) => ByName(name) ?? defaultValue;
    }
}