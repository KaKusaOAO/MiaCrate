using MiaCrate.Client.Models;
using MiaCrate.World.Entities;

namespace MiaCrate.Client.Graphics;

public abstract class RenderLayer<T, TModel> where T : Entity where TModel : IEntityModel<T>
{
    private readonly IRenderLayerParent<T, TModel> _renderer;

    public TModel ParentModel => _renderer.Model;
    
    protected RenderLayer(IRenderLayerParent<T, TModel> renderer)
    {
        _renderer = renderer;
    }

    protected ResourceLocation GetTextureLocation(T entity) => _renderer.GetTextureLocation(entity);

    public abstract void Render(PoseStack stack, IMultiBufferSource bufferSource, int i, T entity, 
        float f, float g, float h, float j, float k, float l);
}