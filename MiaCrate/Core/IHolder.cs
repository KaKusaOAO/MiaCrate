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
}

public static class Holder
{
    public static IHolder<T> Direct<T>(T obj) where T : class => new DirectHolder<T>(obj);
}