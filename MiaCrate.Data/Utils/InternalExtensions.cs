using Mochi.Utils;

namespace MiaCrate.Data.Utils;

internal static class InternalExtensions
{
    public static TValue ComputeIfAbsent<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
        Func<TKey, TValue> mapper)
    {
        if (dict.TryGetValue(key, out var stored)) return stored;
        
        var value = mapper(key);
        dict[key] = value;
        return value;
    }

    public static IOptional<TOut> SelectMany<T, TOut>(this IOptional<T> self, Func<T, IOptional<TOut>> mapper) => 
        self.IsEmpty ? Optional.Empty<TOut>() : mapper(self.Value);
}