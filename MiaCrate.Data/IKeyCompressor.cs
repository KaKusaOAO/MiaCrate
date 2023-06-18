using Mochi.Utils;

namespace MiaCrate.Data;

public interface IKeyCompressor
{
    int Compress(string key);
    int Size { get; }
}

public interface IKeyCompressor<T> : IKeyCompressor
{
    T Decompress(int key);
    int Compress(T key);
}

public sealed class KeyCompressor<T> : IKeyCompressor<T> where T : notnull
{
    private readonly Dictionary<int, T> _decompress = new();
    private readonly Dictionary<T, int> _compress = new();
    private readonly Dictionary<string, int> _compressString = new();
    private readonly IDynamicOps<T> _ops;
    public int Size { get; }

    public KeyCompressor(IDynamicOps<T> ops, IEnumerable<T> keys)
    {
        _ops = ops;
        
        foreach (var key in keys)
        {
            if (_compress.ContainsKey(key)) continue;
            var next = _compress.Count;
            _compress[key] = next;

            _ops.GetStringValue(key).Result.IfPresent(k =>
            {
                _compressString[k] = next;
            });

            _decompress[next] = key;
        }

        Size = _compress.Count;
    }

    public T Decompress(int key) => _decompress[key];

    public int Compress(string key)
    {
        var id = _compressString.GetValueOrDefault(key, -1);
        return id == -1 ? Compress(_ops.CreateString(key)) : id;
    }

    public int Compress(T key) => _compress[key];
}