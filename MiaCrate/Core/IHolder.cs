using MiaCrate.Data;
using MiaCrate.Resources;
using MiaCrate.Tags;
using Mochi.Utils;

namespace MiaCrate.Core;

public interface IHolder
{
    public object Value { get; }
    public bool IsBound { get; }
    public HolderKind Kind { get; }

    public bool Is(ResourceLocation location);
}

public interface IHolder<T> : IHolder where T : class
{
    public new T Value { get; }
    object IHolder.Value => Value!;
    
    public bool Is(IResourceKey<T> key);
    public bool Is(Predicate<IResourceKey<T>> predicate);
    public bool Is(ITagKey<T> tagKey);

    public IEither<IResourceKey<T>, T> Unwrap();
    public IOptional<IResourceKey<T>> UnwrapKey();

    public bool CanSerializeIn(IHolderOwner<T> owner);
}

public static class Holder
{
    public static IHolder<T> Direct<T>(T obj) where T : class => new DirectHolder<T>(obj);

    public static class Reference
    {
        public static IReferenceHolder<T> CreateStandalone<T>(IHolderOwner<T> registry, IResourceKey<T> key) where T : class => 
            new ReferenceHolder<T>(RefHolderType.Standalone, registry, key, null);
        
        public static IReferenceHolder<T> CreateIntrusive<T>(IHolderOwner<T> registry, T obj) where T : class => 
            new ReferenceHolder<T>(RefHolderType.Intrusive, registry, null, obj);
    }

    private class DirectHolder<T> : IDirectHolder<T> where T : class
    {
        public T Value { get; }

        public DirectHolder(T value)
        {
            Value = value;
        }

        public IEither<IResourceKey<T>, T> Unwrap() => Either.CreateRight(Value)
            .Left<IResourceKey<T>>();

        public IOptional<IResourceKey<T>> UnwrapKey() => Optional.Empty<IResourceKey<T>>();

        public bool CanSerializeIn(IHolderOwner<T> owner) => true;
    }
    
    private class ReferenceHolder<T> : IReferenceHolder<T> where T : class
    {
        private HashSet<ITagKey<T>> _tags = new();

        public T Value { get; private set; }

        public IResourceKey<T> Key { get; private set; }
        public bool IsBound => Key != null! && Value != null!;
        public RefHolderType Type { get; }
        public IHolderOwner<T> Owner { get; }

        public ReferenceHolder(RefHolderType type, IHolderOwner<T> owner, IResourceKey<T>? key, T? obj)
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
        public bool Is(ITagKey<T> tagKey) => _tags.Contains(tagKey);

        public IEither<IResourceKey<T>, T> Unwrap() => Either.CreateLeft(Key).Right<T>();

        public IOptional<IResourceKey<T>> UnwrapKey() => Optional.Of(Key);
    
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

        public void BindTags(IEnumerable<ITagKey<T>> tags)
        {
            _tags = new HashSet<ITagKey<T>>(tags);
        }

        public bool CanSerializeIn(IHolderOwner<T> owner) => Owner.CanSerializeIn(owner);
    }
}