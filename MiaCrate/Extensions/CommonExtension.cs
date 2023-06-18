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
}