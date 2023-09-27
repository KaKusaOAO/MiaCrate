using MiaCrate.Client.Fonts;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Client.UI;

public sealed class GlyphProviderType : IEnumLike<GlyphProviderType>, IStringRepresentable
{
    private static readonly Dictionary<int, GlyphProviderType> _values = new();

    public static readonly GlyphProviderType Bitmap = new("bitmap", 
        BitmapProvider.Definition.Codec.Cast<IGlyphProviderDefinition>());

    public static readonly GlyphProviderType Space = new("space",
        SpaceProvider.Definition.Codec.Cast<IGlyphProviderDefinition>());

    public static readonly GlyphProviderType Reference = new("reference",
        ProviderReferenceDefinition.Codec.Cast<IGlyphProviderDefinition>());

    public static GlyphProviderType[] Values => _values.Values.ToArray();

    public static ICodec<GlyphProviderType> Codec { get; } =
        IStringRepresentable.FromEnum<GlyphProviderType>();
    
    public string SerializedName { get; }
    public IMapCodec<IGlyphProviderDefinition> MapCodec { get; }

    public int Ordinal { get; }

    private GlyphProviderType(string name, IMapCodec<IGlyphProviderDefinition> codec)
    {
        SerializedName = name;
        MapCodec = codec;

        Ordinal = _values.Count;
        _values[Ordinal] = this;
    }
}