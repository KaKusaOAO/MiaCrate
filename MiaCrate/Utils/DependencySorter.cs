using MiaCrate.Auth;

namespace MiaCrate;

public class DependencySorter<T, TEntry> where TEntry : IDependencySorterEntry<T>
{
    private readonly Dictionary<T, TEntry> _contents = new();

    public DependencySorter<T, TEntry> AddEntry(T obj, TEntry entry)
    {
        _contents[obj] = entry;
        return this;
    }

    private static bool IsCyclic(Multimap<T, T> map, T key, T key2)
    {
        var set = map[key2];
        return set.Contains(key) || set.Any(oo => IsCyclic(map, key, oo));
    }

    private static void AddDependencyIfNotCyclic(Multimap<T, T> map, T key, T key2)
    {
        if (IsCyclic(map, key, key2)) return;
        map.Add(key, key2);
    }

    private void VisitDependenciesAndElement(Multimap<T, T> map, HashSet<T> set, T obj, Action<T, TEntry> consumer)
    {
        if (!set.Add(obj)) return;
        foreach (var x1 in map[obj])
        {
            VisitDependenciesAndElement(map, set, x1, consumer);
        }

        if (_contents.TryGetValue(obj, out var entry))
        {
            consumer(obj, entry);
        }
    }

    public void OrderByDependencies(Action<T, TEntry> consumer)
    {
        var multimap = new Multimap<T, T>();
        foreach (var (obj, entry) in _contents)
        {
            entry.VisitRequiredDependencies(o => AddDependencyIfNotCyclic(multimap, obj, o));
        }
        
        foreach (var (obj, entry) in _contents)
        {
            entry.VisitOptionalDependencies(o => AddDependencyIfNotCyclic(multimap, obj, o));
        }

        var set = new HashSet<T>();
        foreach (var key in _contents.Keys)
        {
            VisitDependenciesAndElement(multimap, set, key, consumer);
        }
    }
}