using MiaCrate.Client.Models;
using MiaCrate.World.Entities;

namespace MiaCrate.Client.Graphics;

public static class ItemInHandLayer
{
    public static ItemInHandLayer<T, TModel> Create<T, TModel>(IRenderLayerParent<T, TModel> renderer,
        ItemInHandRenderer itemInHandRenderer) where T : LivingEntity where TModel : IEntityModel<T>, IArmedModel =>
        new(renderer, itemInHandRenderer);
}

public class ItemInHandLayer<T, TModel> : RenderLayer<T, TModel> 
    where T : LivingEntity where TModel : IEntityModel<T>, IArmedModel
{
    private readonly ItemInHandRenderer _itemInHandRenderer;

    public ItemInHandLayer(IRenderLayerParent<T, TModel> renderer, ItemInHandRenderer itemInHandRenderer) : base(renderer)
    {
        _itemInHandRenderer = itemInHandRenderer;
    }

    public override void Render(PoseStack stack, IMultiBufferSource bufferSource, int i, T entity, float f, float g, float h, float j,
        float k, float l)
    {
        throw new NotImplementedException();
    }
}