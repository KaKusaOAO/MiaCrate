using MiaCrate.Client.Systems;
using MiaCrate.Resources;

namespace MiaCrate.Client.Graphics;

public class PreloadedTexture : SimpleTexture
{
    private Task<TextureImage>? _task;
    
    public PreloadedTexture(IResourceManager manager, ResourceLocation location, IExecutor executor) : base(location)
    {
        var source = new TaskCompletionSource<TextureImage>();
        executor.Execute(() =>
        {
            var image = TextureImage.Load(manager, location);
            source.SetResult(image);
        });
        _task = source.Task;
    }

    protected override TextureImage GetTextureImage(IResourceManager manager)
    {
        if (_task == null)
            return TextureImage.Load(manager, Location);
        
        var image = _task.Result;
        _task = null;
        return image;
    }

    public override void Reset(TextureManager textureManager, IResourceManager resourceManager, ResourceLocation location,
        IExecutor executor)
    {
        var source = new TaskCompletionSource<TextureImage>();
        executor.Execute(() =>
        {
            var image = TextureImage.Load(resourceManager, location);
            source.SetResult(image);
        });
        
        _task = source.Task;
        _task.ContinueWith(_ =>
        {
            CreateExecutor(executor).Execute(() =>
            {
                textureManager.Register(location, this);
            });
        });
    }

    private static IExecutor CreateExecutor(IExecutor executor)
    {
        return IExecutor.Create(r =>
        {
            executor.Execute(() =>
            {
                RenderSystem.RecordRenderCall(r.Run);
            });
        });
    }

    public Task Task => _task ?? Task.CompletedTask;
}