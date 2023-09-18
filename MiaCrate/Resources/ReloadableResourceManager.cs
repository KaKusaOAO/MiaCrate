using Mochi.Utils;

namespace MiaCrate.Resources;

public class ReloadableResourceManager : IDisposableResourceManager
{
    private IDisposableResourceManager _resources;
    private readonly List<IPreparableReloadListener> _listeners = new();
    private readonly PackType _type;

    public ReloadableResourceManager(PackType type)
    {
        _type = type;
        _resources = new MultiPackResourceManager(type, Enumerable.Empty<IPackResources>().ToList());
    }

    public void RegisterReloadListener(IPreparableReloadListener reloadListener)
    {
        _listeners.Add(reloadListener);
    }
    
    public IReloadInstance CreateReload(IExecutor executor, IExecutor executor2, Task task, List<IPackResources> list)
    {
        var packs = string.Join(", ", list.Select(p => p.PackId));
        Logger.Info($"Reloading ResourceManager: {packs}");
        
        _resources.Dispose();
        _resources = new MultiPackResourceManager(_type, list);
        return SimpleReloadInstance.Create(_resources, _listeners, executor, executor2, task, Logger.Level <= LogLevel.Verbose);
    }
    
    public IOptional<Resource> GetResource(ResourceLocation location) => _resources.GetResource(location);

    public HashSet<string> Namespaces => _resources.Namespaces;

    public List<Resource> GetResourceStack(ResourceLocation location) => _resources.GetResourceStack(location);

    public Dictionary<ResourceLocation, Resource> ListResources(string path, Predicate<ResourceLocation> predicate) => 
        _resources.ListResources(path, predicate);

    public Dictionary<ResourceLocation, List<Resource>> ListResourceStacks(string path,
        Predicate<ResourceLocation> predicate) =>
        _resources.ListResourceStacks(path, predicate);

    public IEnumerable<IPackResources> Packs => _resources.Packs;
    
    public void Dispose()
    {
        _resources.Dispose();
    }
}