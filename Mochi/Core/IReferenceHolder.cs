using Mochi.Resources;

namespace Mochi.Core;

public interface IReferenceHolder : IHolder
{
    public IResourceKey Key { get; }

    HolderKind IHolder.Kind => HolderKind.Reference;

    public void Bind(IResourceKey key, object value);
}

public interface IReferenceHolder<T> : IHolder<T>, IReferenceHolder where T : class
{
    public new IResourceKey<T> Key { get; }
    IResourceKey IReferenceHolder.Key => Key;

    public void Bind(IResourceKey<T> key, T value);
    void IReferenceHolder.Bind(IResourceKey key, object value) => Bind((IResourceKey<T>) key, (T) value);
}

public class ReferenceHolder<T> : IReferenceHolder<T> where T : class
{
    public T Value { get; private set; }
    public IResourceKey<T>? Key { get; private set; }
    public bool IsBound { get; }
    public RefHolderType Type { get; }
    public IRegistry<T> Registry { get; }

    private ReferenceHolder(RefHolderType type, IRegistry<T> registry, IResourceKey<T>? key, T? obj)
    {
        Type = type;
        Registry = registry;
        Value = obj!;
        Key = key;
    }

    public static IReferenceHolder<T> CreateStandalone(IRegistry<T> registry, IResourceKey<T> key) => 
        new ReferenceHolder<T>(RefHolderType.Standalone, registry, key, null);
    
    public static IReferenceHolder<T> CreateIntrusive(IRegistry<T> registry, T obj) => 
        new ReferenceHolder<T>(RefHolderType.Intrusive, registry, null, obj);

    public bool Is(ResourceLocation location) => Key!.Location == location;
    
    public void Bind(IResourceKey<T> key, T value)
    {
        if (Key != null && Key != key)
        {
            throw new InvalidOperationException($"Can't change holder key: existing={Key}, new={key}");
        }

        if (Value != null && Value != value)
        {
            throw new InvalidOperationException($"Can't change holder {key} value: existing={Value}, new={value}");
        }

        Key = key;
        Value = value;
    }
}

public enum RefHolderType
{
    /// <summary>
    /// 
    /// </summary>
    Standalone,
    
    /// <summary>
    /// 
    /// </summary>
    Intrusive
}