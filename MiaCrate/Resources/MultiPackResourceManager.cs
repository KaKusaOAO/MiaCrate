using Mochi.Utils;

namespace MiaCrate.Resources;

public class MultiPackResourceManager : IDisposableResourceManager
{
    private readonly List<IPackResources> _packs;

    public MultiPackResourceManager(PackType type, List<IPackResources> packs)
    {
        _packs = packs.ToList();
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

    public Dictionary<ResourceLocation, Resource> ListResources(string str, Predicate<ResourceLocation> predicate)
    {
        throw new NotImplementedException();
    }

    public Dictionary<ResourceLocation, List<Resource>> ListResourceStacks(string str, Predicate<ResourceLocation> predicate)
    {
        throw new NotImplementedException();
    }

    public List<IPackResources> Packs => throw new NotImplementedException();

    public void Dispose()
    {
        // throw new NotImplementedException();
    }
}