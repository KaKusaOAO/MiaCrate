using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class DynamicTexture : AbstractTexture, IDumpable
{
    private NativeImage? _pixels;

    public NativeImage? Pixels
    {
        get => _pixels;
        set
        {
            _pixels?.Dispose();
            _pixels = value;
        }
    }
    
    public DynamicTexture(NativeImage image)
    {
        _pixels = image;

        void ExecuteRenderCall()
        {
            var preparation = TextureUtil.PrepareImage(_pixels!.Width, _pixels.Height);
            _textureDescription = preparation.TextureDescription;
            _samplerDescription = preparation.SamplerDescription;
            
            var factory = GlStateManager.ResourceFactory;
            Texture?.Dispose();
            Texture = factory.CreateTexture(_textureDescription);
        
            Sampler?.Dispose();
            Sampler = factory.CreateSampler(_samplerDescription);
            
            Upload();
        }
        
        if (!RenderSystem.IsOnRenderThread)
        {
            RenderSystem.RecordRenderCall(ExecuteRenderCall);
        }
        else
        {
            ExecuteRenderCall();
        }
    }

    public DynamicTexture(int width, int height, bool clearBuffer)
    {
        RenderSystem.AssertOnGameThreadOrInit();
        _pixels = new NativeImage(width, height, clearBuffer);
        
        var preparation = TextureUtil.PrepareImage(_pixels.Width, _pixels.Height);
        _textureDescription = preparation.TextureDescription;
        _samplerDescription = preparation.SamplerDescription;
        
        var factory = GlStateManager.ResourceFactory;
        Texture?.Dispose();
        Texture = factory.CreateTexture(_textureDescription);
        
        Sampler?.Dispose();
        Sampler = factory.CreateSampler(_samplerDescription);
    }

    public void Upload()
    {
        if (_pixels == null)
        {
            Logger.Warn($"Trying to upload disposed texture {Texture}");
            return;
        }

        _pixels.Upload(this, 0, 0, 0, false);
    }

    public override void Load(IResourceManager manager)
    {
        
    }

    public void DumpContents(ResourceLocation location, string path)
    {
        if (_pixels == null) return;
        
        // TODO: dump
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_pixels == null) return;
        
        _pixels.Dispose();
        ReleaseId();
        _pixels = null;
    }
}