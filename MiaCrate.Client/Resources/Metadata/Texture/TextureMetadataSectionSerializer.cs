using System.Text.Json.Nodes;
using MiaCrate.Resources;

namespace MiaCrate.Client.Resources;

public class TextureMetadataSectionSerializer : IMetadataSectionSerializer<TextureMetadataSection>
{
    public TextureMetadataSection FromJson(JsonObject obj)
    {
        var blur = obj["blur"]?.GetValue<bool>() ?? false;
        var clamp = obj["clamp"]?.GetValue<bool>() ?? false;
        return new TextureMetadataSection(blur, clamp);
    }

    public string MetadataSectionName => "texture";
}