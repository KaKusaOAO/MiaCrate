using MiaCrate.Resources;

namespace MiaCrate.Core;

public class DefaultedRegistry<T> : MappedRegistry<T> where T : class
{
    public ResourceLocation DefaultKey { get; }
    private IHolder<T> _defaultValue;
    
    public DefaultedRegistry(string defaultKey, IResourceKey<IRegistry> key, Func<T, IReferenceHolder<T>>? provider) : base(key, provider)
    {
        DefaultKey = defaultKey;
    }

    public override IHolder<T> RegisterMapping(int id, IResourceKey<T> key, T obj)
    {
        var holder = base.RegisterMapping(id, key, obj);
        if (DefaultKey == key.Location)
        {
            _defaultValue = holder;
        }

        return holder;
    }

    public override T Get(ResourceLocation location) => base.Get(location) ?? _defaultValue.Value;

    public override int GetId(T obj)
    {
        var i = base.GetId(obj);
        return i == -1 ? base.GetId(_defaultValue.Value) : i;
    }

    public override ResourceLocation GetKey(T obj) => base.GetKey(obj) ?? DefaultKey;
}