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

    public static IOptional<T> Where<T>(this IOptional<T> optional, Predicate<T> predicate)
    {
        if (optional.IsEmpty) return optional;
        return predicate(optional.Value) ? optional : Optional.Empty<T>();
    }

    public static IOptional<T> FindFirst<T>(this IEnumerable<T> source) => 
        source
            .Select(Optional.Of)
            .Append(Optional.Empty<T>())
            .First();
}