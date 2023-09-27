using MiaCrate.Client.UI;
using MiaCrate.Core;
using MiaCrate.World.Entities;
using MiaCrate.World.Phys;

namespace MiaCrate.Client.Graphics;

public abstract class EntityRenderer<T> : IEntityRenderer<T> where T : Entity
{
    protected EntityRenderDispatcher EntityRenderDispatcher { get; }
    public Font Font { get; } 
    protected float ShadowRadius { get; set; }
    
    protected EntityRenderer(EntityRendererContext context)
    {
        EntityRenderDispatcher = context.EntityRenderDispatcher;
        Font = context.Font;
    }

    public int GetPackedLightCoords(T entity, float f)
    {
        throw new NotImplementedException();
    }

    public bool ShouldRender(T entity, Frustum frustum, double x, double y, double z)
    {
        throw new NotImplementedException();
    }

    public Vec3 GetRenderOffset(T entity, float f)
    {
        throw new NotImplementedException();
    }

    protected virtual int GetBlockLightLevel(T entity, BlockPos pos)
    {
        throw new NotImplementedException();
    }

    public virtual void Render(T entity, float f, float g, PoseStack stack, IMultiBufferSource bufferSource, int i)
    {
        throw new NotImplementedException();
    }

    protected virtual bool ShouldShowName(T entity)
    {
        throw new NotImplementedException();
    }

    public abstract ResourceLocation GetTextureLocation(T entity);
}