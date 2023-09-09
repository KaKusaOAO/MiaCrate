using MiaCrate.Data;
using MiaCrate.IO;
using MiaCrate.Resources;
using Mochi.Texts;

namespace MiaCrate.Client.Resources;

public class ClientPackSource : BuiltInPackSource
{
    private static readonly IComponent _vanillaName = TranslateText.Of("resourcePack.vanilla.name");
    private static readonly ResourceLocation _packsDir = new("resourcepacks");

    private static readonly PackMetadataSection _versionMetadataSection = new(
        TranslateText.Of("resourcePack.vanilla.description"),
        SharedConstants.CurrentVersion.GetPackVersion(PackType.ClientResources)
    );
    
    private static readonly BuiltInMetadata _builtInMetadata = 
        BuiltInMetadata.Of(PackMetadataSection.Type, _versionMetadataSection); 
    
    public ClientPackSource(IFileSystem path) : base(PackType.ClientResources, CreateVanillaPackSource(path), _packsDir)
    {
    }

    private static VanillaPackResources CreateVanillaPackSource(IFileSystem path)
    {
        return new VanillaPackResourcesBuilder()
            .SetMetadata(_builtInMetadata)
            .ExposeNamespace("minecraft", "realms")
            .ApplyDevelopmentConfig()
            .PushJarResources()
            .PushAssetPath(PackType.ClientResources, path)
            .Build();
    }

    protected override Pack? CreateVanillaPack(IPackResources resources) =>
        Pack.ReadMetaAndCreate("vanilla", _vanillaName, true, _ => resources, PackType.ClientResources,
            PackPosition.Bottom, IPackSource.BuiltIn);
}