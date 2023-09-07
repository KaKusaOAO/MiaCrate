namespace MiaCrate.Resources;

public class Resource
{
    public IPackResources Source { get; }
    private readonly Func<Stream> _streamSupplier;
    private readonly Func<IResourceMetadata> _metadataSupplier;

    public Resource(IPackResources source, Func<Stream> streamSupplier, Func<IResourceMetadata> metadataSupplier)
    {
        Source = source;
        _streamSupplier = streamSupplier;
        _metadataSupplier = metadataSupplier;
    }

    public Stream Open() => _streamSupplier();
}