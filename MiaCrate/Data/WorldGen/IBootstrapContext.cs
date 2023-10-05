using MiaCrate.Core;
using MiaCrate.Resources;

namespace MiaCrate.Data.WorldGen;

public interface IBootstrapContext
{
    
}

public interface IBootstrapContext<T> : IBootstrapContext where T : class
{
    public IReferenceHolder<T> Register(IResourceKey<T> key, T obj, Lifecycle lifecycle);
    public IHolderGetter<TS> Lookup<TS>(IResourceKey<IRegistry<TS>> key) where TS : class;
}