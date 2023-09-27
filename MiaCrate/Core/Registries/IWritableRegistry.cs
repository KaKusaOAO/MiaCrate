using MiaCrate.Data;
using MiaCrate.Resources;

namespace MiaCrate.Core;

public interface IWritableRegistry : IRegistry
{
    public bool IsEmpty { get; }
    
    public IReferenceHolder Register(IResourceKey key, object obj, Lifecycle lifecycle);
    public IHolderGetter CreateRegistrationLookup();
}

public interface IWritableRegistry<T> : IRegistry<T>, IWritableRegistry where T : class
{
    public IReferenceHolder<T> Register(IResourceKey<T> key, T obj, Lifecycle lifecycle);
    IReferenceHolder IWritableRegistry.Register(IResourceKey key, object obj, Lifecycle lifecycle) => 
        Register((IResourceKey<T>) key, (T) obj, lifecycle);
}