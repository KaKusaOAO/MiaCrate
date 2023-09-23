using MiaCrate.Client.Graphics;

namespace MiaCrate.Client.Resources;

public interface IUnbakedModel
{
    public IEnumerable<ResourceLocation> Dependencies { get; }
    public void ResolveParents(Func<ResourceLocation, IUnbakedModel> func);

    public IBakedModel? Bake(IModelBaker modelBaker, Func<Material, TextureAtlasSprite> func, IModelState modelState,
        ResourceLocation location);
}