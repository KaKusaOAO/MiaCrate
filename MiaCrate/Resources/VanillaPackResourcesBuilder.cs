namespace MiaCrate.Resources;

public class VanillaPackResourcesBuilder
{
    private BuiltInMetadata _metadata;
    private readonly HashSet<string> _namespaces = new();

    public VanillaPackResourcesBuilder SetMetadata(BuiltInMetadata metadata)
    {
        _metadata = metadata;
        return this;
    }

    public VanillaPackResourcesBuilder ExposeNamespace(params string[] namespaces)
    {
        foreach (var ns in namespaces)
        {
            _namespaces.Add(ns);
        }

        return this;
    }

    public VanillaPackResourcesBuilder ApplyDevelopmentConfig()
    {
        return this;
    }

    public VanillaPackResourcesBuilder PushJarResources()
    {
        return this;
    }

    public VanillaPackResourcesBuilder PushAssetPath(PackType type, string path)
    {
        return this;
    }

    public VanillaPackResources Build()
    {
        return new VanillaPackResources(_metadata, _namespaces, new List<string>(),
            new Dictionary<PackType, List<string>>());
    }
}