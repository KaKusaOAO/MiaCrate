using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using MiaCrate.Resources;
using Veldrid;

namespace MiaCrate.Client.Graphics;

public abstract class AbstractTexture : IDisposable
{
    public const int NotAssigned = -1;
    
    private int _id = NotAssigned;
    protected bool IsBlur { get; set; }
    protected bool IsMipmap { get; set; }

    public Texture? Texture { get; internal set; }
    public Sampler? Sampler { get; internal set; }

    internal TextureDescription _textureDescription;
    internal SamplerDescription _samplerDescription;

    public void SetFilter(bool blur, bool mipmap)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        IsBlur = blur;
        IsMipmap = mipmap;
        
        if (blur)
        {
            _samplerDescription.Filter = SamplerFilter.MinLinear_MagLinear_MipLinear;
        }
        else
        {
            _samplerDescription.Filter = SamplerFilter.MinPoint_MagPoint_MipPoint;
        }
        
        Sampler?.Dispose();
        Sampler = GlStateManager.ResourceFactory.CreateSampler(_samplerDescription);
    }

    public void ReleaseId()
    {
        void ExecuteRenderCall()
        {
            Texture?.Dispose();
            Sampler?.Dispose();
            Texture = null;
            Sampler = null;
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
    
    /// <exception cref="IOException"></exception>
    public abstract void Load(IResourceManager manager);

    public void Bind()
    {
        
    }

    public virtual void Reset(TextureManager textureManager, IResourceManager resourceManager,
        ResourceLocation location, IExecutor executor)
    {
        textureManager.Register(location, this);
    }
    
    public virtual void Dispose()
    {
    }
}