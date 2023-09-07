using System.Collections;
using System.Text.Json.Nodes;

namespace MiaCrate.Auth;

public class PropertyMap : IDictionary<string, HashSet<Property>>
{
    private readonly Dictionary<string, HashSet<Property>> _delegate = new();

    public IEnumerator<KeyValuePair<string, HashSet<Property>>> GetEnumerator() => _delegate.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void ICollection<KeyValuePair<string, HashSet<Property>>>.Add(KeyValuePair<string, HashSet<Property>> item) =>
        ((ICollection<KeyValuePair<string, HashSet<Property>>>) _delegate).Add(item);

    public void Clear() => _delegate.Clear();

    bool ICollection<KeyValuePair<string, HashSet<Property>>>.Contains(KeyValuePair<string, HashSet<Property>> item) =>
        _delegate.Contains(item);

    void ICollection<KeyValuePair<string, HashSet<Property>>>.CopyTo(KeyValuePair<string, HashSet<Property>>[] array,
        int arrayIndex) =>
        ((ICollection<KeyValuePair<string, HashSet<Property>>>) _delegate).CopyTo(array, arrayIndex);

    bool ICollection<KeyValuePair<string, HashSet<Property>>>.Remove(KeyValuePair<string, HashSet<Property>> item) =>
        ((ICollection<KeyValuePair<string, HashSet<Property>>>) _delegate).Remove(item);

    public int Count => _delegate.Count;

    public bool IsReadOnly => false;

    void IDictionary<string, HashSet<Property>>.Add(string key, HashSet<Property> value) =>
        _delegate.Add(key, value);

    public void Add(string key, Property value)
    {
        if (_delegate.TryGetValue(key, out var set))
        {
            set.Add(value);
            return;
        }

        var newSet = new HashSet<Property> {value};
        _delegate.Add(key, newSet);
    }

    public bool ContainsKey(string key) => _delegate.ContainsKey(key);

    public bool Remove(string key) => _delegate.Remove(key);

    public bool Remove(string key, Property value)
    {
        if (_delegate.TryGetValue(key, out var set))
        {
            set.Remove(value);
            return set.Any() || _delegate.Remove(key);
        }

        return false;
    }

    public bool TryGetValue(string key, out HashSet<Property> value) => 
        _delegate.TryGetValue(key, out value);

    public HashSet<Property> this[string key]
    {
        get => _delegate[key];
        set => _delegate[key] = value;
    }

    public ICollection<string> Keys => _delegate.Keys;

    public ICollection<HashSet<Property>> Values => _delegate.Values;

    public static PropertyMap FromJson(JsonNode? node)
    {
        var result = new PropertyMap();
        if (node == null) return result;

        if (node is JsonObject obj)
        {
            foreach (var (key, value) in obj)
            {
                if (value is JsonArray arr)
                {
                    foreach (var element in arr)
                    {
                        result.Add(key, new Property(key, element!.GetValue<string>()));
                    }
                }   
            }
        } else if (node is JsonArray arr)
        {
            foreach (var element in arr)
            {
                if (element is JsonObject item)
                {
                    var name = item["name"]!.GetValue<string>();
                    var val = item["value"]!.GetValue<string>();
                    result.Add(name, new Property(name, val, item["signature"]?.GetValue<string>()));
                }
            }
        }

        return result;
    }

    public JsonNode ToJson()
    {
        var result = new JsonArray();
        foreach (var property in Values.SelectMany(v => v))
        {
            var obj = new JsonObject
            {
                ["name"] = property.Name,
                ["value"] = property.Value
            };

            if (property.HasSignature)
            {
                obj["signature"] = property.Signature;
            }
            
            result.Add(obj);
        }

        return result;
    }
}