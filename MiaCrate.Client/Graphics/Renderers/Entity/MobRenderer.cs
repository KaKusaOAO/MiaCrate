using MiaCrate.Client.Models;
using MiaCrate.World.Entities;

namespace MiaCrate.Client.Graphics;

public abstract class MobRenderer<T, TModel> : LivingEntityRenderer<T, TModel>
    where T : Mob where TModel : IEntityModel<T>
{
    public const int LeashRenderSteps = 24;
    
    protected MobRenderer(EntityRendererContext context, TModel model, float shadowRadius) 
        : base(context, model, shadowRadius)
    {
    }
    
    
}