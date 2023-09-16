using Mochi.Utils;

namespace MiaCrate.Resources;

public interface IResourceMetadata
{
    public static readonly IResourceMetadata Empty = new EmptyMetadata();
    public static readonly Func<IResourceMetadata> EmptySupplier = () => Empty;
    
    public IOptional<T> GetSection<T>(IMetadataSectionSerializer<T> serializer);

    private class EmptyMetadata : IResourceMetadata
    {
        public IOptional<T> GetSection<T>(IMetadataSectionSerializer<T> serializer) => Optional.Empty<T>();
    }
}