using MiaCrate.Data;
using MiaCrate.Resources;
using MiaCrate.Tags;
using Mochi.Utils;

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

    bool IHolder<T>.Is(IResourceKey<T> key) => Key == key;
    bool IHolder<T>.Is(Predicate<IResourceKey<T>> predicate) => predicate(Key);

    public void BindKey(IResourceKey<T> key);
    void IReferenceHolder.BindKey(IResourceKey key) => BindKey((IResourceKey<T>)key);
    
    public void BindValue(T value);
    void IReferenceHolder.BindValue(object value) => BindValue((T)value);

    public void BindTags(IEnumerable<ITagKey<T>> tags);
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