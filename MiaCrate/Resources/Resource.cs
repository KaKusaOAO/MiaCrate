namespace MiaCrate.Resources;

public class Resource
{
    private readonly Func<Stream> _streamSupplier;
    private readonly Func<IResourceMetadata> _metadataSupplier;
    private IResourceMetadata? _cachedMetadata;
    
    public IPackResources Source { get; }
    public string SourcePackId => Source.PackId;

    public Resource(IPackResources source, Func<Stream> streamSupplier, Func<IResourceMetadata> metadataSupplier)
    {
        Source = source;
        _streamSupplier = streamSupplier;
        _metadataSupplier = metadataSupplier;
    }

    public Resource(IPackResources source, Func<Stream> streamSupplier)
    {
        Source = source;
        _streamSupplier = streamSupplier;
        _metadataSupplier = IResourceMetadata.EmptySupplier;
        _cachedMetadata = IResourceMetadata.Empty;
    }

    public Stream Open() => _streamSupplier();

    public IResourceMetadata Metadata => _cachedMetadata ??= _metadataSupplier();
}