using MiaCrate.Resources;

namespace MiaCrate.Core;

public interface IRegistryLookup<T> : IHolderLookup<T>, IHolderOwner<T> where T : class
{
    public IResourceKey<IRegistry> Key { get; }
}