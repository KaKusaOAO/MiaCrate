using System.Text.Json.Nodes;
using MiaCrate.Resources;

namespace MiaCrate.Client.Resources;

public class TextureMetadataSectionSerializer : IMetadataSectionSerializer<TextureMetadataSection>
{
    public TextureMetadataSection FromJson(JsonObject obj)
    {
        var blur = obj["blur"]!.GetValue<bool>();
        var clamp = obj["clamp"]!.GetValue<bool>();
        return new TextureMetadataSection(blur, clamp);
    }

    public string MetadataSectionName => "texture";
}