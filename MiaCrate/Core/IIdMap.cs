using System.Collections;

namespace MiaCrate.Core;

public interface IIdMap<T> : IEnumerable<T>
{
    public int GetId(T obj);
    public T? ById(int id);

    public T ByIdOrThrow(int id)
    {
        var obj = ById(id);
        if (obj == null)
            throw new ArgumentException($"No value with ID {id}");
        return obj;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}