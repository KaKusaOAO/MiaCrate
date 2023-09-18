using System.Collections;
using MiaCrate.Extensions;
using Mochi.Utils;

namespace MiaCrate.Resources;

public class MultiPackResourceManager : IDisposableResourceManager
{
    private readonly Dictionary<string, FallbackResourceManager> _namespacedManagers;
    private readonly List<IPackResources> _packs;

    public HashSet<string> Namespaces => _namespacedManagers.Keys.ToHashSet();
    
    public IEnumerable<IPackResources> Packs => _packs;

    public MultiPackResourceManager(PackType type, List<IPackResources> packs)
    {
        _packs = packs.ToList();
        
        var dict = new Dictionary<string, FallbackResourceManager>();
        var list = packs.SelectMany(r => r.GetNamespaces(type)).Distinct().ToArray();
        
        foreach (var resources in packs)
        {
            var section = GetPackFilterSection(resources);
            var set = resources.GetNamespaces(type);
            var predicate = section != null
                ? new Predicate<ResourceLocation>(location => section.IsPathFiltered(location.Path))
                : null;
            
            foreach (var s in list)
            {
                var bl = set.Contains(s);
                var bl2 = section != null && section.IsNamespaceFiltered(s);
                if (!bl && !bl2) continue;

                var manager = dict.ComputeIfAbsent(s, _ => new FallbackResourceManager(type, s));
                
                switch (bl)
                {
                    case true when bl2:
                        manager.Push(resources, predicate);
                        break;
                    case true:
                        manager.Push(resources);
                        break;
                    default:
                        manager.PushFilterOnly(resources.PackId, predicate);
                        break;
                }
            }
        }

        _namespacedManagers = dict;
    }

    private ResourceFilterSection? GetPackFilterSection(IPackResources resources)
    {
        try
        {
            return resources.GetMetadataSection(ResourceFilterSection.Type);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to get filter section from pack {resources.PackId}");
            return null;
        }
    }
    
    public IOptional<Resource> GetResource(ResourceLocation location)
    {
        return _namespacedManagers.GetValueOrDefault(location.Namespace)?.GetResource(location) ??
               Optional.Empty<Resource>();
    }

    public List<Resource> GetResourceStack(ResourceLocation location)
    {
        return _namespacedManagers.GetValueOrDefault(location.Namespace)?.GetResourceStack(location) ??
               new List<Resource>();
    }

    public Dictionary<ResourceLocation, Resource> ListResources(string path, Predicate<ResourceLocation> predicate)
    {
        CheckTrailingDirectoryPath(path);
        return _namespacedManagers.Values
            .SelectMany(m => m.ListResources(path, predicate))
            .ToDictionarySkipDuplicates(e => e.Key, e => e.Value);
    }

    public Dictionary<ResourceLocation, List<Resource>> ListResourceStacks(string path, Predicate<ResourceLocation> predicate)
    {
        CheckTrailingDirectoryPath(path);
        return _namespacedManagers.Values
            .SelectMany(m => m.ListResourceStacks(path, predicate))
            .ToDictionarySkipDuplicates(e => e.Key, e => e.Value);
    }

    private static void CheckTrailingDirectoryPath(string path)
    {
        if (path.EndsWith("/"))
            throw new ArgumentException($"Trailing slash in path {path}");
    }

    public void Dispose()
    {
        // throw new NotImplementedException();
    }
}