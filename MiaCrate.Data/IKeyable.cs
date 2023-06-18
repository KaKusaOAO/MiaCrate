using System.Collections;

namespace MiaCrate.Data;

public interface IKeyable
{
    public IEnumerable<T> Keys<T>(IDynamicOps<T> ops);
}