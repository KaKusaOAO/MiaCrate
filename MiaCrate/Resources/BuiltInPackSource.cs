namespace MiaCrate.Resources;

public abstract class BuiltInPackSource : IRepositorySource
{
    private readonly PackType _type;
    private readonly ResourceLocation _packDir;

    public VanillaPackResources VanillaPack { get; }

    protected BuiltInPackSource(PackType type, VanillaPackResources pack, ResourceLocation packDir)
    {
        _type = type;
        VanillaPack = pack;
        _packDir = packDir;
    }

    public void LoadPacks(Action<Pack> loader)
    {
        var pack = CreateVanillaPack(VanillaPack);
        if (pack != null) loader(pack);
        
        
    }

    protected abstract Pack? CreateVanillaPack(IPackResources resources);

    private void ListBundledPacks(Action<Pack> loader)
    {
        
    }

    protected void PopulatePackList(Action<string, Func<string, Pack>> populate)
    {
        // _pack
    }
}