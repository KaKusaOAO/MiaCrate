using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Core;

public interface IHolderGetter
{
    
}

public interface IHolderGetter<T> : IHolderGetter where T : class
{
    public IOptional<IReferenceHolder<T>> Get(IResourceKey<T> key);

    public IReferenceHolder<T> GetOrThrow(IResourceKey<T> key) => 
        Get(key).OrElseGet(() => throw new Exception($"Missing element {key}"));
}