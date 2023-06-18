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
}