namespace MiaCrate.Data;

public interface IMapLike
{
    
}

public interface IMapLike<T> : IMapLike
{
    T? this[T key] { get; }
    T? this[string key] { get; }
    IEnumerable<IPair<T, T>> Entries { get; }
}

public static class MapLike
{
    public static IMapLike<T> ForDictionary<T>(IDictionary<T, T> dict, IDynamicOps<T> ops) => 
        new DictMapLike<T>(dict, ops);

    private class DictMapLike<T> : IMapLike<T>
    {
        private readonly IDictionary<T, T> _dict;
        private readonly IDynamicOps<T> _ops;

        public DictMapLike(IDictionary<T, T> dict, IDynamicOps<T> ops)
        {
            _dict = dict;
            _ops = ops;
        }

        public T? this[T key] => _dict.TryGetValue(key, out var result) ? result : default;
        public T? this[string key]
        {
            get
            {
                var k = _ops.CreateString(key);
                return _dict.TryGetValue(k, out var result) ? result : default;
            }
        }

        public IEnumerable<IPair<T, T>> Entries => _dict.Select(e => Pair.Of(e.Key, e.Value));
        public override string ToString() => $"MapLike[{_dict}]";
    }
}