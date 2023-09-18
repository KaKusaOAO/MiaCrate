using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Extensions;

namespace MiaCrate.Client.Graphics;

public static class SpriteSources
{
    private static readonly Dictionary<ResourceLocation, SpriteSourceType> _types = new();

    public static readonly SpriteSourceType SingleFile = Register("single", Graphics.SingleFile.Codec);

    public static readonly ICodec<SpriteSourceType> TypeCodec =
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

    public static readonly ICodec<ISpriteSource> Codec = 
        TypeCodec.Dispatch(s => s.Type, t => t.Codec);

    public static readonly ICodec<List<ISpriteSource>> FileCodec =
        Codec.ListCodec.FieldOf("sources").Codec;
            
    private static SpriteSourceType Register(string name, ICodec<ISpriteSource> codec)
    {
        var type = new SpriteSourceType(codec);
        var location = new ResourceLocation(name);
        var other = _types.ComputeIfAbsent(location, _ => type);
        if (other != null)
            throw new InvalidOperationException($"Duplicate registration {location}");

        return type;
    }

    private static SpriteSourceType Register<T>(string name, ICodec<T> codec) where T : ISpriteSource =>
        Register(name, codec.Cast<ISpriteSource>());
}