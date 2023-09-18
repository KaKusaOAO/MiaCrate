namespace MiaCrate.Resources;

public class PackRepository
{
    private readonly HashSet<IRepositorySource> _sources;
    private Dictionary<string, Pack> _available = new();
    private List<Pack> _selected = new();

    public PackRepository(params IRepositorySource[] sources)
    {
        _sources = sources.ToHashSet();
    }
    
    public void Reload()
    {
        var list = _selected.Select(p => p.Id).ToList();
        _available = DiscoverAvailable();
        _selected = RebuildSelected(list);
    }

    private Dictionary<string, Pack> DiscoverAvailable()
    {
        var map = new Dictionary<string, Pack>();
        foreach (var source in _sources)
        {
            source.LoadPacks(p => map[p.Id] = p);
        }

        return map;
    }

    private List<Pack> GetAvailablePacks(List<string> ids) => ids
        .Select(x => _available[x])
        .Where(x => x != null).ToList();

    public List<string> SelectedIds => _selected.Select(p => p.Id).ToList();
    public List<string> AvailableIds => _available.Keys.ToList();
    public List<Pack> AvailablePacks => _available.Values.ToList();
    
    private List<Pack> RebuildSelected(List<string> ids)
    {
        var list = GetAvailablePacks(ids);
        foreach (var pack in _available.Values)
        {
            if (pack.IsRequired && list.Contains(pack))
            {
                pack.DefaultPosition.Insert(list, pack, x => x, false);
            }
        }

        return list;
    }

    public List<IPackResources> OpenAllSelected() => _selected.Select(x => x.Open()).ToList();
}