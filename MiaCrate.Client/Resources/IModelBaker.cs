using MiaCrate.Client.Models;

namespace MiaCrate.Client.Resources;

public interface IModelBaker
{
    public IUnbakedModel GetModel(ResourceLocation location);
    public IBakedModel? Bake(ResourceLocation location, IModelState modelState);
}