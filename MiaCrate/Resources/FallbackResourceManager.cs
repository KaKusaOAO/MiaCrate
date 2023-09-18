using Mochi.Utils;

namespace MiaCrate.Resources;

public class FallbackResourceManager : IResourceManager
{
    private readonly PackType _type;
    private readonly string _namespace;

    public FallbackResourceManager(PackType type, string ns)
    {
        _type = type;
        _namespace = ns;
    }

    public void Push(IPackResources resources)
    {
        
    }

    public void Push(IPackResources resources, Predicate<ResourceLocation>? predicate)
    {
        
    }

    public void PushFilterOnly(string str, Predicate<ResourceLocation>? predicate)
    {
        
    }

    private void PushInternal(string str, IPackResources? resources, Predicate<ResourceLocation>? predicate)
    {
        
    }

    public IOptional<Resource> GetResource(ResourceLocation location)
    {
        throw new NotImplementedException();
    }

    public HashSet<string> Namespaces => throw new NotImplementedException();

    public List<Resource> GetResourceStack(ResourceLocation location)
    {
        throw new NotImplementedException();
    }

    public Dictionary<ResourceLocation, Resource> ListResources(string path, Predicate<ResourceLocation> predicate)
    {
        throw new NotImplementedException();
    }

    public Dictionary<ResourceLocation, List<Resource>> ListResourceStacks(string path, Predicate<ResourceLocation> predicate)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IPackResources> Packs => throw new NotImplementedException();
}