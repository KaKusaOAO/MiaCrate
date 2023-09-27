using MiaCrate.Core;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Resources;
using Mochi.Extensions;
using Mochi.Utils;

namespace MiaCrate.Tags;

public interface ITagKey
{
    private static readonly Dictionary<int, ITagKey> _interned = new();
    
    public IResourceKey<IRegistry> Registry { get; }
    public ResourceLocation Location { get; }
    
    public static ICodec<ITagKey<T>> Codec<T>(IResourceKey<IRegistry<T>> key) where T : class
    {
        return ResourceLocation.Codec.CrossSelect(
            l => Create(key, l), 
            k => k.Location);
    }
    
    public static ICodec<ITagKey<T>> HashedCodec<T>(IResourceKey<IRegistry<T>> key) where T : class
    {
        const string hash = "#";
        return Data.Codec.String.CoSelectSelectMany(
            s => s!.StartsWith(hash)
                ? ResourceLocation.Read(s[1..]).Select(l => Create(key, l))
                : DataResult.Error<ITagKey<T>>(() => "Not a tag id"),
            t => $"{hash}{t.Location}");
    }

    public static ITagKey<T> Create<T>(IResourceKey<IRegistry<T>> key, ResourceLocation location) where T : class
    {
        var result = new TagKey<T>(key, location);
        
        // This should work
        return (ITagKey<T>) _interned.ComputeIfAbsent(result.GetHashCode(), _ => result);
    }

    public bool IsFor(IResourceKey<IRegistry> key) => Registry == key;

    public IOptional<ITagKey<T>> Cast<T>(IResourceKey<IRegistry<T>> key) where T : class =>
        IsFor(key) ? Optional.Of((ITagKey<T>) this) : Optional.Empty<ITagKey<T>>();

    private record TagKey<T>(IResourceKey<IRegistry<T>> Registry, ResourceLocation Location) : ITagKey<T> where T : class;
}

public interface ITagKey<T> : ITagKey where T : class
{
    public new IResourceKey<IRegistry<T>> Registry { get; }
    IResourceKey<IRegistry> ITagKey.Registry => Registry;
}