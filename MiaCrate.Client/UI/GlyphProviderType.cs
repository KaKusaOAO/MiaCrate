using MiaCrate.Data.Codecs;

namespace MiaCrate.Client.UI;

public sealed class GlyphProviderType : IStringRepresentable
{
    public static readonly GlyphProviderType Bitmap = new("bitmap", 
        BitmapProvider.Definition.Codec.Cast<IGlyphProviderDefinition>());
    
    public string SerializedName { get; }
    public IMapCodec<IGlyphProviderDefinition> Codec { get; }

    private GlyphProviderType(string name, IMapCodec<IGlyphProviderDefinition> codec)
    {
        SerializedName = name;
        Codec = codec;
    }
}