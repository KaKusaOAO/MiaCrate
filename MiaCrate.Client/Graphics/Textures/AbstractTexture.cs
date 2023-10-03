using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using MiaCrate.Common;
using MiaCrate.Resources;
using Mochi.Extensions;
using Veldrid;

namespace MiaCrate.Client.Graphics;

public abstract class AbstractTexture : IDisposable
{
    public const int NotAssigned = -1;
    
    private int _id = NotAssigned;
    protected bool IsBlur { get; set; }
    protected bool IsMipmap { get; set; }

    public TextureInstance? Texture { get; protected set; }

    public void SetFilter(bool blur, bool mipmap)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        IsBlur = blur;
        IsMipmap = mipmap;
        
        Texture?.ModifySampler((ref SamplerDescription s) =>
        {
            if (blur)
            {
                s.Filter = SamplerFilter.MinLinear_MagLinear_MipLinear;
            }
            else
            {
                s.Filter = SamplerFilter.MinPoint_MagPoint_MipPoint;
            }
        });
        Texture?.EnsureSamplerUpToDate();
    }
    
    public void SetAddressMode(SamplerAddressMode u, SamplerAddressMode v)
    {
        Texture?.ModifySampler((ref SamplerDescription s) =>
        {
            s.AddressModeU = u;
            s.AddressModeV = v;
        });
    }

    public void ReleaseId()
    {
        void ExecuteRenderCall()
        {
            Texture?.Dispose();
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