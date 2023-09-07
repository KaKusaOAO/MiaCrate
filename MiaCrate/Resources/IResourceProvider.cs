using Mochi.Utils;

namespace MiaCrate.Resources;

public interface IResourceProvider
{
    public IOptional<Resource> GetResource(ResourceLocation location);

    public static IResourceProvider Create(Func<ResourceLocation, IOptional<Resource>> func) => new Instance(func);
    
    public static IResourceProvider FromDictionary(IDictionary<ResourceLocation, Resource> map) => new Mapped(map);
    public Resource GetResourceOrThrow(ResourceLocation location) =>
        GetResource(location).OrElse(() => throw new FileNotFoundException(location));

    public Stream Open(ResourceLocation location) =>
        GetResourceOrThrow(location).Open();

    private class Mapped : IResourceProvider
    {
        private readonly IDictionary<ResourceLocation, Resource> _map;

        public Mapped(IDictionary<ResourceLocation, Resource> map)
        {
            _map = map;
        }

        public IOptional<Resource> GetResource(ResourceLocation location)
        {
            return _map.TryGetValue(location, out var result)
                ? Optional.Of(result)
                : Optional.Empty<Resource>();
        }
    }
    
    private class Instance : IResourceProvider
    {
        private readonly Func<ResourceLocation, IOptional<Resource>> _del;

        public Instance(Func<ResourceLocation, IOptional<Resource>> del)
        {
            _del = del;
        }

        public IOptional<Resource> GetResource(ResourceLocation location) => _del(location);
    }
}