using MiaCrate.Client.Systems;
using MiaCrate.Resources;
using MiaCrate.Texts;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class PreloadedTexture : SimpleTexture
{
    private Task<TextureImage>? _task;
    
    public PreloadedTexture(IResourceManager manager, ResourceLocation location, IExecutor executor) : base(location)
    {
        _task = Tasks.SupplyAsync(() => TextureImage.Load(manager, location), executor);
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
        _task = Tasks.SupplyAsync(() => TextureImage.Load(resourceManager, Location), Util.BackgroundExecutor);
        _task.ThenRunAsync(() =>
        {
            textureManager.Register(Location, this);
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