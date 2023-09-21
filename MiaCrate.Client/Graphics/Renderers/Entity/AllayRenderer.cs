using MiaCrate.Client.Models;
using MiaCrate.Core;
using MiaCrate.World.Entities;

namespace MiaCrate.Client.Graphics;

public class AllayRenderer : MobRenderer<Allay, AllayModel>
{
    private static readonly ResourceLocation _texture = new("textures/entity/allay/allay.png");
    
    public AllayRenderer(EntityRendererContext context) 
        : base(context, new AllayModel(), 0.4f)
    {
        AddLayer(ItemInHandLayer.Create(this, context.ItemInHandRenderer));
    }

    public override ResourceLocation GetTextureLocation(Allay entity) => _texture;

    protected override int GetBlockLightLevel(Allay entity, BlockPos pos) => 15;
}