using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using MiaCrate.Common;
using MiaCrate.Resources;
using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Graphics;

public abstract class AbstractTexture : IDisposable
{
    public const int NotAssigned = -1;
    
    private int _id = NotAssigned;
    protected bool IsBlur { get; set; }
    protected bool IsMipmap { get; set; }

    public int Id
    {
        get
        {
            RenderSystem.AssertOnRenderThreadOrInit();
            if (_id == NotAssigned)
            {
                _id = TextureUtil.GenerateTextureId();
            }

            return _id;
        }
    }

    public void SetFilter(bool blur, bool mipmap)
    {
        RenderSystem.AssertOnRenderThreadOrInit();
        IsBlur = blur;
        IsMipmap = mipmap;

        if (blur)
        {
            GlStateManager.TexMinFilter(TextureTarget.Texture2D, 
                mipmap ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear);
            GlStateManager.TexMagFilter(TextureTarget.Texture2D, TextureMagFilter.Linear);
        }
        else
        {
            GlStateManager.TexMinFilter(TextureTarget.Texture2D, 
                mipmap ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.Nearest);
            GlStateManager.TexMagFilter(TextureTarget.Texture2D, TextureMagFilter.Nearest);
        }
    }

    public void ReleaseId()
    {
        void ExecuteRenderCall()
        {
            if (_id != NotAssigned)
            {
                TextureUtil.ReleaseTextureId(_id);
                _id = NotAssigned;
            }
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
        if (!RenderSystem.IsOnRenderThreadOrInit)
        {
            RenderSystem.RecordRenderCall(() =>
            {
                GlStateManager.BindTexture(Id);
            });
        }
        else
        {
            GlStateManager.BindTexture(Id);
        }
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