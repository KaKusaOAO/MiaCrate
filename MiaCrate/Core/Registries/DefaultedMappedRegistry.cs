using MiaCrate.Data;
using MiaCrate.Resources;

namespace MiaCrate.Core;

public class DefaultedMappedRegistry<T> : MappedRegistry<T>, IDefaultedRegistry where T : class
{
    public ResourceLocation DefaultKey { get; }
    private IHolder<T>? _defaultValue;
    
    public DefaultedMappedRegistry(string defaultKey, IResourceKey<IRegistry> key, Lifecycle lifecycle, bool hasIntrusiveHolders) 
        : base(key, lifecycle, hasIntrusiveHolders)
    {
        DefaultKey = defaultKey;
    }

    public override IReferenceHolder<T> RegisterMapping(int id, IResourceKey<T> key, T obj, Lifecycle lifecycle)
    {
        var holder = base.RegisterMapping(id, key, obj, lifecycle);
        if (DefaultKey == key.Location)
        {
            _defaultValue = holder;
        }

        return holder;
    }

    private T GetDefaultValue() => _defaultValue?.Value ??
                                   throw new InvalidOperationException("Default value not exist in registry");
    
    public override T Get(ResourceLocation location) => base.Get(location) ?? GetDefaultValue();

    public override int GetId(T obj)
    {
        var i = base.GetId(obj);
        return i == -1 ? base.GetId(GetDefaultValue()) : i;
    }

    public override ResourceLocation GetKey(T obj) => base.GetKey(obj) ?? DefaultKey;
}