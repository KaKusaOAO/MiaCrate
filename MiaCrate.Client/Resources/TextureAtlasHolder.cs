using MiaCrate.Client.Graphics;
using MiaCrate.Resources;

namespace MiaCrate.Client.Resources;

public abstract class TextureAtlasHolder : IPreparableReloadListener, IDisposable
{
    private readonly ResourceLocation _atlasInfoLocation;
    private readonly IEnumerable<IMetadataSectionSerializer> _serializers;
    private readonly TextureAtlas _textureAtlas;

    protected TextureAtlasHolder(TextureManager manager, ResourceLocation textureAtlas, ResourceLocation atlasInfoLocation)
        : this(manager, textureAtlas, atlasInfoLocation, SpriteLoader.DefaultMetadataSections) {}
    
    protected TextureAtlasHolder(TextureManager manager, ResourceLocation textureAtlas, ResourceLocation atlasInfoLocation,
        IEnumerable<IMetadataSectionSerializer> serializers)
    {
        _atlasInfoLocation = atlasInfoLocation;
        _textureAtlas = new TextureAtlas(textureAtlas);
        manager.Register(_textureAtlas.Location, _textureAtlas);
        _serializers = serializers;
    }

    public virtual TextureAtlasSprite GetSprite(ResourceLocation location)
    {
        return _textureAtlas.GetSprite(location);
    }
    
    public Task ReloadAsync(IPreparableReloadListener.IPreparationBarrier barrier, IResourceManager manager, IProfilerFiller preparationProfiler,
        IProfilerFiller reloadProfiler, IExecutor preparationExecutor, IExecutor reloadExecutor)
    {
        return SpriteLoader.Create(_textureAtlas)
            .LoadAndStitchAsync(manager, _atlasInfoLocation, 0, preparationExecutor, _serializers)
            .ThenComposeAsync(p => p.WaitForUploadAsync())
            .ThenComposeAsync(barrier.Wait)
            .ThenAcceptAsync(p => Apply(p, reloadProfiler), reloadExecutor);
    }

    private void Apply(SpriteLoader.Preparations preparations, IProfilerFiller profiler)
    {
        profiler.StartTick();
        profiler.Push("upload");
        _textureAtlas.Upload(preparations);
        profiler.Pop();
        profiler.EndTick();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}