using Mochi.Utils;

namespace MiaCrate.Resources;

public interface IResourceMetadata
{
    public IOptional<T> GetSection<T>(IMetadataSectionSerializer<T> serializer);
}