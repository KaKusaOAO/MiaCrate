using Mochi.Texts;

namespace MiaCrate.Resources;

public class ClientPackSource : BuiltInPackSource
{
    private static readonly ResourceLocation _packsDir = new("resourcepacks");
    private static readonly BuiltInMetadata _builtInMetadata = BuiltInMetadata.Of(); 
    
    public ClientPackSource(string path) : base(PackType.ClientResources, CreateVanillaPackSource(path), _packsDir)
    {
    }

    private static VanillaPackResources CreateVanillaPackSource(string path)
    {
        return new VanillaPackResourcesBuilder()
            .SetMetadata(_builtInMetadata)
            .ExposeNamespace("minecraft", "realms")
            .ApplyDevelopmentConfig()
            .PushJarResources()
            .PushAssetPath(PackType.ClientResources, path)
            .Build();
    }

    protected override Pack? CreateVanillaPack(IPackResources resources)
    {
        throw new NotImplementedException();
    }
}

public class PackMetadataSection
{
    public static readonly IMetadataSectionType<PackMetadataSection> Type = new PackMetadataSectionSerializer();
    
    public IComponent Description { get; }
    public int PackFormat { get; }

    public PackMetadataSection(IComponent description, int packFormat)
    {
        Description = description;
        PackFormat = packFormat;
    }
}