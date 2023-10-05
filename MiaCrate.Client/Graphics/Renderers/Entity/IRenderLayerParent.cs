using MiaCrate.Client.Models;
using MiaCrate.World.Entities;

namespace MiaCrate.Client.Graphics;

public interface IRenderLayerParent<in T, out TModel> where TModel : IEntityModel<T> where T : Entity
{
    public TModel Model { get; }
    public ResourceLocation GetTextureLocation(T entity);
}