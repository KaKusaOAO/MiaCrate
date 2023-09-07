using MiaCrate.Resources;

namespace MiaCrate.Core;

public interface IReferenceHolder : IHolder
{
    public IResourceKey Key { get; }

    HolderKind IHolder.Kind => HolderKind.Reference;

    public void BindKey(IResourceKey key);
    public void BindValue(object value);
}

public interface IReferenceHolder<T> : IHolder<T>, IReferenceHolder where T : class
{
    public new IResourceKey<T> Key { get; }
    IResourceKey IReferenceHolder.Key => Key;

    public void BindKey(IResourceKey<T> key);
    void IReferenceHolder.BindKey(IResourceKey key) => BindKey((IResourceKey<T>)key);
    
    public void BindValue(T value);
    void IReferenceHolder.BindValue(object value) => BindValue((T)value);
}

public class ReferenceHolder<T> : IReferenceHolder<T> where T : class
{
    public T Value { get; private set; }
    public IResourceKey<T> Key { get; private set; }
    public bool IsBound => Key != null! && Value != null!;
    public RefHolderType Type { get; }
    public IHolderOwner<T> Owner { get; }

    private ReferenceHolder(RefHolderType type, IHolderOwner<T> owner, IResourceKey<T>? key, T? obj)
    {
        Type = type;
        Owner = owner;
        Value = obj!;
        Key = key!;
    }

    public static IReferenceHolder<T> CreateStandalone(IHolderOwner<T> registry, IResourceKey<T> key) => 
        new ReferenceHolder<T>(RefHolderType.Standalone, registry, key, null);
    
    public static IReferenceHolder<T> CreateIntrusive(IHolderOwner<T> registry, T obj) => 
        new ReferenceHolder<T>(RefHolderType.Intrusive, registry, null, obj);

    public bool Is(ResourceLocation location) => Key!.Location == location;
    
    public void BindKey(IResourceKey<T> key)
    {
        if (Key != null && Key != key)
        {
            throw new InvalidOperationException($"Can't change holder key: existing={Key}, new={key}");
        }

        Key = key;
    }
    
    public void BindValue(T value)
    {
        if (Value != null && Value != value)
        {
            throw new InvalidOperationException($"Can't change holder {Key} value: existing={Value}, new={value}");
        }

        Value = value;
    }

    public bool CanSerializeIn(IHolderOwner<T> owner) => Owner.CanSerializeIn(owner);
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