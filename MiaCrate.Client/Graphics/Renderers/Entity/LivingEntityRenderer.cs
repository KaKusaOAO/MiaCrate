using MiaCrate.Client.Models;
using MiaCrate.World.Entities;

namespace MiaCrate.Client.Graphics;

public abstract class LivingEntityRenderer<T, TModel> : EntityRenderer<T>, IRenderLayerParent<T, TModel> 
    where T : LivingEntity where TModel : IEntityModel<T>
{
    protected readonly List<RenderLayer<T, TModel>> _layers = new();

    public TModel Model { get; protected set; }
    
    protected LivingEntityRenderer(EntityRendererContext context, TModel model, float shadowRadius) : base(context)
    {
        Model = model;
        ShadowRadius = shadowRadius;
    }

    protected void AddLayer(RenderLayer<T, TModel> renderLayer)
    {
        _layers.Add(renderLayer);
    }
    
    protected virtual void Scale(T entity, PoseStack stack, float f) {}
}