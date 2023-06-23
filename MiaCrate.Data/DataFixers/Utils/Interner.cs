namespace MiaCrate.Data;

internal class Interner<T>
{
    private readonly List<T> _list = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public T Intern(T obj)
    {
        _lock.Wait();
        var interned = _list.FirstOrDefault(t => t!.Equals(obj));
        if (interned != null) return interned;
        
        _list.Add(obj);
        _lock.Release();
        return obj;
    }
}