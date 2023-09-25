using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using MiaCrate.Resources;
using Mochi.Utils;
using OpenTK.Graphics.OpenGL4;

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
            GlStateManager.ObjectLabel(ObjectLabelIdentifier.Texture, Id, $"DynamicTexture #{Id}");
            TextureUtil.PrepareImage(Id, _pixels!.Width, _pixels.Height);
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
        
        GlStateManager.ObjectLabel(ObjectLabelIdentifier.Texture, Id, $"DynamicTexture #{Id}");
        TextureUtil.PrepareImage(Id, _pixels.Width, _pixels.Height);
    }

    public void Upload()
    {
        if (_pixels == null)
        {
            Logger.Warn($"Trying to upload disposed texture {Id}");
            return;
        }

        Bind();
        _pixels.Upload(0, 0, 0, false);
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