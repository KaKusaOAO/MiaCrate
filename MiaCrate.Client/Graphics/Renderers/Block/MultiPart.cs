using System.Text.Json.Nodes;
using MiaCrate.Client.Models;
using MiaCrate.Client.Resources;
using MiaCrate.World.Blocks;

namespace MiaCrate.Client.Graphics;

public class MultiPart : IUnbakedModel
{
    private readonly IStateDefinition<Block, BlockState> _definition;
    
    public List<Selector> Selectors { get; }

    public IEnumerable<MultiVariant> MultiVariants => Selectors
        .Select(s => s.Variant).Distinct();

    public IEnumerable<ResourceLocation> Dependencies => Selectors
        .SelectMany(s => s.Variant.Dependencies).Distinct();

    public MultiPart(IStateDefinition<Block, BlockState> definition, List<Selector> selectors)
    {
        _definition = definition;
        Selectors = selectors;
    }

    public void ResolveParents(Func<ResourceLocation, IUnbakedModel> func)
    {
        foreach (var selector in Selectors)
        {
            selector.Variant.ResolveParents(func);
        }
    }

    public IBakedModel? Bake(IModelBaker modelBaker, Func<Material, TextureAtlasSprite> func, IModelState modelState, ResourceLocation location)
    {
        throw new NotImplementedException();
    }

    public static MultiPart FromJson(BlockModelDefinition.Context context, JsonArray arr) => 
        new(context.Definition, GetSelectors(arr));

    private static List<Selector> GetSelectors(JsonArray arr) => 
        arr.Select(Selector.FromJson).ToList();
}