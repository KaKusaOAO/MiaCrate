using System.Text.Json.Nodes;
using MiaCrate.Resources;

namespace MiaCrate.Client.Resources;

public class AnimationMetadataSectionSerializer : IMetadataSectionSerializer<AnimationMetadataSection>
{
    public string MetadataSectionName => AnimationMetadataSection.SectionName;
    
    public AnimationMetadataSection FromJson(JsonObject obj)
    {
        throw new NotImplementedException();
    }
}