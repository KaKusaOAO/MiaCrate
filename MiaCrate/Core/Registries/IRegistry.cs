using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Core;

public interface IRegistry : IKeyable
{
    public IResourceKey<IRegistry> Key { get; }
    public int GetId(object obj);
    public object? ById(int id);
    public object? Get(IResourceKey key);
    public object? Get(ResourceLocation location);
    public IOptional GetOptional(IResourceKey key);
    public IOptional GetOptional(ResourceLocation location);
    public ResourceLocation? GetKey(object obj);
    public IOptional<IResourceKey> GetResourceKey(object obj);
    public Lifecycle Lifecycle(object obj);
    public ISet<ResourceLocation> KeySet { get; }
    public IRegistry Freeze();

    IEnumerable<T> IKeyable.GetKeys<T>(IDynamicOps<T> ops) => 
        KeySet.Select(r => ops.CreateString(r));
}

public interface IRegistry<T> : IRegistry, IIdMap<T> where T : class
{
    public new int GetId(T obj);
    int IRegistry.GetId(object obj) => GetId((T) obj);

    public new T? ById(int id);
    object? IRegistry.ById(int id) => ById(id);
    T? IIdMap<T>.ById(int id) => ById(id);
    
    public T? Get(IResourceKey<T> key);
    object? IRegistry.Get(IResourceKey key) => Get((IResourceKey<T>) key);
    
    public new T? Get(ResourceLocation location);
    object? IRegistry.Get(ResourceLocation location) => Get(location);
    
    public IOptional<T> GetOptional(IResourceKey<T> key) => Optional.OfNullable(Get(key));
    IOptional IRegistry.GetOptional(IResourceKey key) => GetOptional((IResourceKey<T>) key);
    
    public new IOptional<T> GetOptional(ResourceLocation location) => Optional.OfNullable(Get(location));
    IOptional IRegistry.GetOptional(ResourceLocation location) => GetOptional(location);

    public ResourceLocation? GetKey(T obj);
    ResourceLocation? IRegistry.GetKey(object obj) => GetKey((T) obj);
    
    public IOptional<IResourceKey<T>> GetResourceKey(T obj);
    IOptional<IResourceKey> IRegistry.GetResourceKey(object obj) => GetResourceKey((T) obj);
    
    public Lifecycle Lifecycle(T obj);
    Lifecycle IRegistry.Lifecycle(object obj) => Lifecycle((T) obj);

    public IOptional<IReferenceHolder<T>> GetHolder(int id);
    public IOptional<IReferenceHolder<T>> GetHolder(IResourceKey<T> key);
    
    public new IRegistry<T> Freeze();
    IRegistry IRegistry.Freeze() => Freeze();
    
    public IReferenceHolder<T> CreateIntrusiveHolder(T obj);
    
    public IHolderOwner<T> HolderOwner { get; }
    public IHolderLookup<T> AsLookup();

    public ICodec<T> ByNameCodec
    {
        get
        {
            var codec = ResourceLocation.Codec.FlatCrossSelect(
                l => GetOptional(l)
                    .Select(DataResult.Success)
                    .OrElseGet(() =>
                        DataResult.Error<T>(() => $"Unknown registry key in {Key}: {l}")),
                o => GetResourceKey(o)
                    .Select(k => k.Location)
                    .Select(DataResult.Success)
                    .OrElseGet(() =>
                        DataResult.Error<ResourceLocation>(() => $"Unknown registry element in {Key}: {o}"))
            );

            const int unknownId = -1;
            var codec2 = ExtraCodecs.IdResolverCodec(
                o => GetResourceKey(o).IsPresent ? GetId(o) : unknownId,
                ById, unknownId);

            return ExtraCodecs.OverrideLifecycle(
                ExtraCodecs.OrCompressed(codec, codec2),
                Lifecycle, Lifecycle);
        }
    }
}