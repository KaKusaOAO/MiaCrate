using System.IO.Compression;
using MiaCrate.Extensions;
using MiaCrate.Texts;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.Resources;

public class ModPackSource : IRepositorySource
{
    public void LoadPacks(Action<Pack> loader)
    {
        if (!SharedConstants.IncludesMods) return;

        if (SharedConstants.IncludesSodium) 
            Load("sodium", MiaComponent.Literal("Sodium"), ResourceAssembly.SodiumArchive, loader);
        
        if (SharedConstants.IncludesIris) 
            Load("iris", MiaComponent.Literal("Iris"), ResourceAssembly.IrisArchive, loader);
    }

    private void Load(string id, IComponent title, ZipArchive archive, Action<Pack> loader)
    {
        var pack = Pack.Create($"mods/{id}", title, true,
            _ => new ModPackResources(id, archive), 
            new PackInfo(title,
                SharedConstants.ResourcePackFormat, FeatureFlags.VanillaSet),
            PackType.ClientResources, PackPosition.Top,
            false, IPackSource.Default);
            
        Logger.Info(MiaComponent.Translatable("Loading %s pack...", title));
        loader(pack);
    }
}