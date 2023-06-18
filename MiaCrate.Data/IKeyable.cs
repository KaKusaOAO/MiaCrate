using System.Collections;

namespace MiaCrate.Data;

public interface IKeyable
{
    public IEnumerable<T> GetKeys<T>(IDynamicOps<T> ops);
}