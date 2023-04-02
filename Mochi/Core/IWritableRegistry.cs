using Mochi.Resources;

namespace Mochi.Core;

public interface IWritableRegistry : IRegistry
{
    public IHolder Register(IResourceKey location, object obj);
    public IHolder RegisterMapping(int id, IResourceKey location, object obj);
}

public interface IWritableRegistry<T> : IRegistry<T>, IWritableRegistry where T : class
{
    public IHolder<T> Register(IResourceKey<T> key, T obj);
    IHolder IWritableRegistry.Register(IResourceKey key, object obj) => Register((IResourceKey<T>) key, (T) obj)!;
    
    public IHolder<T> RegisterMapping(int id, IResourceKey<T> key, T obj);
    IHolder IWritableRegistry.RegisterMapping(int id, IResourceKey key, object obj) => 
        RegisterMapping(id, (IResourceKey<T>) key, (T) obj)!;
}