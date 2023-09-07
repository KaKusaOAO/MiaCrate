namespace MiaCrate.Core;

public interface IDirectHolder : IHolder
{
    bool IHolder.IsBound => true;
    bool IHolder.Is(ResourceLocation location) => false;
    HolderKind IHolder.Kind => HolderKind.Direct;
}

public interface IDirectHolder<T> : IHolder<T>, IDirectHolder where T : class
{
    
}

public class DirectHolder<T> : IDirectHolder<T> where T : class
{
    public T Value { get; }

    public DirectHolder(T value)
    {
        Value = value;
    }

    public bool CanSerializeIn(IHolderOwner<T> owner) => true;
}