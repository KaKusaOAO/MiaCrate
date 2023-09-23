using System.Text.Json;
using System.Text.Json.Nodes;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.Resources;

public class AnimationMetadataSectionSerializer : IMetadataSectionSerializer<AnimationMetadataSection>
{
    public string MetadataSectionName => AnimationMetadataSection.SectionName;
    
    public AnimationMetadataSection FromJson(JsonObject obj)
    {
        var list = new List<AnimationFrame>();
        var defFrameTime = obj["frametime"]?.GetValue<int>() ?? AnimationMetadataSection.DefaultFrameTime;
        if (defFrameTime != AnimationMetadataSection.DefaultFrameTime)
        {
            if (defFrameTime < 1) 
                throw new JsonException("Invalid default frame time");
        }

        if (obj.TryGetPropertyValue("frames", out var framesNode))
        {
            try
            {
                var arr = framesNode!.AsArray();
                for (var j = 0; j < arr.Count; j++)
                {
                    var node = arr[j];
                    var frame = GetFrame(j, node);
                    if (frame != null) list.Add(frame);
                }
            }
            catch (Exception ex)
            {
                throw new JsonException($"Invalid animation -> frames: expected array, was {framesNode}", ex);
            }
        }

        var width = obj["width"]?.GetValue<int>() ?? AnimationMetadataSection.UnknownSize;
        var height = obj["height"]?.GetValue<int>() ?? AnimationMetadataSection.UnknownSize;

        if (width != AnimationMetadataSection.UnknownSize)
        {
            if (width < 1) throw new JsonException("Invalid width");
        }
        
        if (height != AnimationMetadataSection.UnknownSize)
        {
            if (height < 1) throw new JsonException("Invalid height");
        }

        var interpolate = obj["interpolate"]?.GetValue<bool>() ?? false;
        return new AnimationMetadataSection(list, width, height, defFrameTime, interpolate);
    }

    private AnimationFrame? GetFrame(int i, JsonNode? node)
    {
        if (node is JsonValue value)
        {
            return new AnimationFrame(value.GetValue<int>());
        }

        if (node is JsonObject obj)
        {
            var j = AnimationFrame.UnknownFrameTime;
            if (obj.TryGetPropertyValue("time", out var timeNode))
            {
                j = timeNode!.GetValue<int>();
                if (j < 1) throw new JsonException("Invalid frame time");
            }

            var k = obj["index"]!.GetValue<int>();
            if (k < 0) throw new JsonException("Invalid frame index");

            return new AnimationFrame(k, j);
        }

        return null;
    }
}