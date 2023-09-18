namespace MiaCrate.Resources;

public interface IResourceManager : IResourceProvider
{
    public HashSet<string> Namespaces { get; }
    public List<Resource> GetResourceStack(ResourceLocation location);
    public Dictionary<ResourceLocation, Resource> ListResources(string path, Predicate<ResourceLocation> predicate);
    public Dictionary<ResourceLocation, List<Resource>> ListResourceStacks(string path,
        Predicate<ResourceLocation> predicate);
    public IEnumerable<IPackResources> Packs { get; }
}