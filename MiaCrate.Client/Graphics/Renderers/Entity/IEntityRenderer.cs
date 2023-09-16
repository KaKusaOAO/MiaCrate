using MiaCrate.World.Entities;
using MiaCrate.World.Phys;

namespace MiaCrate.Client.Graphics;

public interface IEntityRenderer
{
    public Font Font { get; }
}

public interface IEntityRenderer<in T> : IEntityRenderer where T : Entity
{
    public int GetPackedLightCoords(T entity, float f);
    public bool ShouldRender(T entity, Frustum frustum, double x, double y, double z);
    public Vec3 GetRenderOffset(T entity, float f);
    public void Render(T entity, float f, float g, PoseStack stack, IMultiBufferSource bufferSource, int i);
    public ResourceLocation GetTextureLocation(T entity);
}