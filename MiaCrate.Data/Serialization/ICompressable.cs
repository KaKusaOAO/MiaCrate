namespace MiaCrate.Data;

public interface ICompressable : IKeyable
{
    IKeyCompressor<T> GetCompressor<T>(IDynamicOps<T> ops);
}