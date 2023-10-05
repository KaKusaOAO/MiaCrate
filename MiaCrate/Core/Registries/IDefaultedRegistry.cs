namespace MiaCrate.Core;

public interface IDefaultedRegistry : IRegistry
{
    public ResourceLocation DefaultKey { get; }
}

public interface IDefaultedRegistry<T> : IRegistry<T>, IDefaultedRegistry where T : class
{
    
}