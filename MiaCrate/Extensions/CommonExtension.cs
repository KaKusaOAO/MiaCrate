using System.Collections;
using Mochi.Core;
using Mochi.Utils;

namespace MiaCrate.Extensions;

public static class CommonExtension
{
    public static TValue ComputeIfAbsent<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
        Func<TKey, TValue> resolver)
    {
        if (dict.TryGetValue(key, out var value)) return value;
        var item = resolver(key);
        dict.Add(key, item);
        return item;
    }

    public static TValue? AddOrSet<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        if (dict.ContainsKey(key))
        {
            var old = dict[key];
            dict[key] = value;
            return old;
        }
        
        dict.Add(key, value);
        return default;
    }

    public static Dictionary<TKey, TValue> ToDictionarySkipDuplicates<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector)
    {
        var dict = new Dictionary<TKey, TValue>();
        foreach (var entry in source)
        {
            dict.ComputeIfAbsent(keySelector(entry), _ => valueSelector(entry));
        }

        return dict;
    }

    public static IOptional<T> FindFirst<T>(this IEnumerable<T> source) => 
        source
            .Select(Optional.Of)
            .Append(Optional.Empty<T>())
            .First();

    public static IEnumerable<T> AsEnumerable<T>(this IOptional<T> source) => new OptionalEnumerable<T>(source);

    public static Predicate<T> Negate<T>(this Predicate<T> source) => o => !source(o);

    private class OptionalEnumerable<T> : IEnumerable<T>
    {
        private readonly IOptional<T> _optional;

        public OptionalEnumerable(IOptional<T> optional)
        {
            _optional = optional;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_optional.IsEmpty) yield break;
            yield return _optional.Value;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}