using MiaCrate.Resources;
using MiaCrate.Tags;
using Mochi.Utils;

namespace MiaCrate.Core;

public interface IHolderGetter
{
    
}

public interface IHolderGetter<T> : IHolderGetter where T : class
{
    public IOptional<IReferenceHolder<T>> Get(IResourceKey<T> key);

    public IOptional<INamedHolderSet<T>> Get(ITagKey<T> tagKey);

    public IReferenceHolder<T> GetOrThrow(IResourceKey<T> key) => 
        Get(key).OrElseGet(() => throw new Exception($"Missing element {key}"));
    
    public INamedHolderSet<T> GetOrThrow(ITagKey<T> tagKey) => 
        Get(tagKey).OrElseGet(() => throw new Exception($"Missing tag {tagKey}"));
}