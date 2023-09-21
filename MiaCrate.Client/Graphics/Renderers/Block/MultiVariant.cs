using System.Text.Json;
using System.Text.Json.Nodes;
using MiaCrate.Client.Models;
using MiaCrate.Client.Resources;

namespace MiaCrate.Client.Graphics;

public class MultiVariant : IUnbakedModel
{
    public List<Variant> Variants { get; }

    public MultiVariant(List<Variant> variants)
    {
        Variants = variants;
    }

    public IEnumerable<ResourceLocation> Dependencies => Variants
        .Select(v => v.ModelLocation)
        .Distinct();

    public void ResolveParents(Func<ResourceLocation, IUnbakedModel> func)
    {
        foreach (var resourceLocation in Variants.Select(v => v.ModelLocation).Distinct())
        {
            func(resourceLocation).ResolveParents(func);
        }
    }

    public IBakedModel? Bake(IModelBaker modelBaker, Func<Material, TextureAtlasSprite> func, IModelState modelState, ResourceLocation location)
    {
        if (!Variants.Any()) return null;
        throw new NotImplementedException();
    }

    public static MultiVariant FromJson(JsonNode? value)
    {
        var list = new List<Variant>();
        if (value is JsonArray arr)
        {
            if (!arr.Any()) 
                throw new JsonException("Empty variant array");

            foreach (var node in arr)
            {
                list.Add(Variant.FromJson(node));
            }
        }
        else
        {
            list.Add(Variant.FromJson(value));    
        }
        
        return new MultiVariant(list);
    }
}