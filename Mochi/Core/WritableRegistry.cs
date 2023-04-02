using Mochi.Resources;

namespace Mochi.Core;

public abstract class WritableRegistry<T> : Registry<T>, IWritableRegistry<T> where T : class
{
    public WritableRegistry(IResourceKey<IRegistry> key) : base(key)
    {
        
    }

    public abstract IHolder<T> Register(IResourceKey<T> key, T obj);
    public abstract IHolder<T> RegisterMapping(int id, IResourceKey<T> key, T obj);
}