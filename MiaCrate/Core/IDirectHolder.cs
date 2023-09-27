using MiaCrate.Data;
using MiaCrate.Resources;
using MiaCrate.Tags;
using Mochi.Utils;

namespace MiaCrate.Core;

public interface IDirectHolder : IHolder
{
    bool IHolder.IsBound => true;
    bool IHolder.Is(ResourceLocation location) => false;
    HolderKind IHolder.Kind => HolderKind.Direct;
}

public interface IDirectHolder<T> : IHolder<T>, IDirectHolder where T : class
{
    bool IHolder<T>.Is(IResourceKey<T> key) => false;
    bool IHolder<T>.Is(Predicate<IResourceKey<T>> predicate) => false;
    bool IHolder<T>.Is(ITagKey<T> tagKey) => false;
}