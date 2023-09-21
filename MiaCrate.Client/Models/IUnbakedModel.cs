using MiaCrate.Client.Graphics;
using MiaCrate.Client.Resources;

namespace MiaCrate.Client.Models;

public interface IUnbakedModel
{
    public IEnumerable<ResourceLocation> Dependencies { get; }
    public void ResolveParents(Func<ResourceLocation, IUnbakedModel> func);

    public IBakedModel? Bake(IModelBaker modelBaker, Func<Material, TextureAtlasSprite> func, IModelState modelState,
        ResourceLocation location);
}