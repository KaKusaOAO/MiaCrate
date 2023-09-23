using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Extensions;

namespace MiaCrate.Client.Graphics;

public static class SpriteSources
{
    private static readonly Dictionary<ResourceLocation, SpriteSourceType> _types = new();

    public static SpriteSourceType SingleFile { get; } = Register("single", Graphics.SingleFile.Codec);
    public static SpriteSourceType Directory { get; } = Register("directory", DirectoryLister.Codec);

    public static SpriteSourceType PalettedPermutations { get; } =
        Register("paletted_permutations", Graphics.PalettedPermutations.Codec);

    public static ICodec<SpriteSourceType> TypeCodec { get; } =
        ResourceLocation.Codec
            .FlatCrossSelect(
                location =>
                {
                    var type = _types.GetValueOrDefault(location);
                    return type != null
                        ? DataResult.Success(type)
                        : DataResult.Error<SpriteSourceType>(() => $"Unknown type {location}");
                },
                type =>
                {
                    var location = _types
                        .ToDictionary(k => k.Value, k => k.Key)
                        .GetValueOrDefault(type);
                    return location != null
                        ? DataResult.Success(location)
                        : DataResult.Error<ResourceLocation>(() => $"Unknown type {location}");
                });

    public static ICodec<ISpriteSource> Codec { get; } = 
        TypeCodec.Dispatch(s => s.Type, t => t.Codec);

    public static ICodec<List<ISpriteSource>> FileCodec { get; } =
        Codec.ListCodec.FieldOf("sources").Codec;
            
    private static SpriteSourceType Register(string name, ICodec<ISpriteSource> codec)
    {
        var type = new SpriteSourceType(codec);
        var location = new ResourceLocation(name);
        if (_types.TryGetValue(location, out _))
            throw new InvalidOperationException($"Duplicate registration {location}");

        _types[location] = type;
        return type;
    }

    private static SpriteSourceType Register<T>(string name, ICodec<T> codec) where T : ISpriteSource =>
        Register(name, codec.Cast<ISpriteSource>());
}