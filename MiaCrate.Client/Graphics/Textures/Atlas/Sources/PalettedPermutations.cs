using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class PalettedPermutations : ISpriteSource
{
    public static ICodec<PalettedPermutations> Codec { get; } =
        RecordCodecBuilder.Create<PalettedPermutations>(instance => instance
            .Group(
                Data.Codec.ListOf(ResourceLocation.Codec).FieldOf("textures")
                    .ForGetter<PalettedPermutations>(p => p.Textures),
                ResourceLocation.Codec.FieldOf("palette_key")
                    .ForGetter<PalettedPermutations>(p => p.PaletteKey),
                Data.Codec.UnboundedMap(Data.Codec.String, ResourceLocation.Codec).FieldOf("permutations")
                    .ForGetter<PalettedPermutations>(p => p.Permutations)
            )
            .Apply(instance, (list, location, p) => new PalettedPermutations(list, location, p))
        );

    public List<ResourceLocation> Textures { get; }
    public ResourceLocation PaletteKey { get; }
    public Dictionary<string, ResourceLocation> Permutations { get; }
    public SpriteSourceType Type => SpriteSources.PalettedPermutations;

    private PalettedPermutations(List<ResourceLocation> textures, ResourceLocation paletteKey,
        Dictionary<string, ResourceLocation> permutations)
    {
        Textures = textures;
        Permutations = permutations;
        PaletteKey = paletteKey;
    }
    
    public void Run(IResourceManager manager, ISpriteSource.IOutput output)
    {
        Util.LogFoobar();
    }
}