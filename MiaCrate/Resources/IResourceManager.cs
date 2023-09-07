namespace MiaCrate.Resources;

public interface IResourceManager : IResourceProvider
{
    public HashSet<string> Namespaces { get; }
    public List<Resource> GetResourceStack(ResourceLocation location);
    public Dictionary<ResourceLocation, Resource> ListResources(string str, Predicate<ResourceLocation> predicate);
    public Dictionary<ResourceLocation, List<Resource>> ListResourceStacks(string str,
        Predicate<ResourceLocation> predicate);
    public List<IPackResources> Packs { get; }
}