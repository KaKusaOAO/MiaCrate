using MiaCrate.Resources;

namespace MiaCrate.Client.Sounds;

public class SoundManager : SimplePreparableReloadListener<SoundManager.Preparations>
{
    private const string SoundsPath = "sounds.json";
    
    private readonly Dictionary<ResourceLocation, Resource> _soundCache = new();
    private readonly SoundEngine _soundEngine;
    
    public SoundManager(Options options)
    {
        _soundEngine = new SoundEngine(this, options, IResourceProvider.FromDictionary(_soundCache));
    }

    protected override Preparations Prepare(IResourceManager manager, IProfilerFiller profiler)
    {
        return new Preparations();
    }

    protected override void Apply(Preparations obj, IResourceManager manager, IProfilerFiller profiler)
    {
        
    }
    
    public class Preparations
    {
        
    }
}