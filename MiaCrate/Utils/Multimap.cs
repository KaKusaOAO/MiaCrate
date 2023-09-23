using System.Collections;
using MiaCrate.Extensions;

namespace MiaCrate;

public class Multimap<TKey, TValue> : IDictionary<TKey, HashSet<TValue>>
{
    private readonly Dictionary<TKey, HashSet<TValue>> _delegate = new();

    public IEnumerator<KeyValuePair<TKey, HashSet<TValue>>> GetEnumerator() => _delegate.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void ICollection<KeyValuePair<TKey, HashSet<TValue>>>.Add(KeyValuePair<TKey, HashSet<TValue>> item) =>
        ((ICollection<KeyValuePair<TKey, HashSet<TValue>>>) _delegate).Add(item);

    public void Clear() => _delegate.Clear();

    bool ICollection<KeyValuePair<TKey, HashSet<TValue>>>.Contains(KeyValuePair<TKey, HashSet<TValue>> item) =>
        _delegate.Contains(item);

    void ICollection<KeyValuePair<TKey, HashSet<TValue>>>.CopyTo(KeyValuePair<TKey, HashSet<TValue>>[] array,
        int arrayIndex) =>
        ((ICollection<KeyValuePair<TKey, HashSet<TValue>>>) _delegate).CopyTo(array, arrayIndex);

    bool ICollection<KeyValuePair<TKey, HashSet<TValue>>>.Remove(KeyValuePair<TKey, HashSet<TValue>> item) =>
        ((ICollection<KeyValuePair<TKey, HashSet<TValue>>>) _delegate).Remove(item);

    public int Count => _delegate.Count;

    public bool IsReadOnly => false;

    void IDictionary<TKey, HashSet<TValue>>.Add(TKey key, HashSet<TValue> value) =>
        _delegate.Add(key, value);

    public void Add(TKey key, TValue value)
    {
        if (_delegate.TryGetValue(key, out var set))
        {
            set.Add(value);
            return;
        }

        var newSet = new HashSet<TValue> {value};
        _delegate.Add(key, newSet);
    }

    public bool ContainsKey(TKey key) => _delegate.ContainsKey(key);

    public bool Remove(TKey key) => _delegate.Remove(key);

    public bool Remove(TKey key, TValue value)
    {
        if (_delegate.TryGetValue(key, out var set))
        {
            set.Remove(value);
            return set.Any() || _delegate.Remove(key);
        }

        return false;
    }

    public bool TryGetValue(TKey key, out HashSet<TValue> value)
    {
        if (_delegate.TryGetValue(key, out value!)) return true;
        value = _delegate[key] = new HashSet<TValue>();
        return true;
    }

    public HashSet<TValue> this[TKey key]
    {
        get => _delegate.ComputeIfAbsent(key, _ => new HashSet<TValue>());
        set => _delegate[key] = value;
    }

    public ICollection<TKey> Keys => _delegate.Keys;

    public ICollection<HashSet<TValue>> Values => _delegate.Values;
}