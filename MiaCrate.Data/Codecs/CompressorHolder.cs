using System.Collections;
using MiaCrate.Data.Utils;

namespace MiaCrate.Data.Codecs;

public abstract class CompressorHolder : ICompressable
{
    private readonly Dictionary<IDynamicOps, IKeyCompressor> _compressors = new();
    public abstract IEnumerable<T> Keys<T>(IDynamicOps<T> ops);

    public IKeyCompressor<T> GetCompressor<T>(IDynamicOps<T> ops) where T : notnull => 
        (IKeyCompressor<T>) _compressors.ComputeIfAbsent(ops, _ => new KeyCompressor<T>(ops, Keys(ops)));
}