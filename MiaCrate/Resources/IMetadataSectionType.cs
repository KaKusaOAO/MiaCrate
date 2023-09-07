using System.ComponentModel;
using System.Text.Json.Nodes;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using Mochi.Texts;
using Component = MiaCrate.Texts.Component;

namespace MiaCrate.Resources;

public interface IMetadataSectionType<T> : IMetadataSectionSerializer<T>
{
    public JsonObject ToJson(T obj);

    public static IMetadataSectionType<T> FromCodec(string name, ICodec<T> codec) =>
        new CodecSectionType(name, codec);

    private class CodecSectionType : IMetadataSectionType<T>
    {
        private readonly string _name;
        private readonly ICodec<T> _codec;

        public CodecSectionType(string name, ICodec<T> codec)
        {
            _name = name;
            _codec = codec;
        }

        public string MetadataSectionName => _name;

        public T FromJson(JsonObject obj) => _codec.Parse(JsonOps.Instance, obj)
            .GetOrThrow(false, _ => { });

        public JsonObject ToJson(T obj) => _codec.EncodeStart(JsonOps.Instance, obj)
            .GetOrThrow(false, _ => { }).AsObject();
    }
}

public class PackMetadataSectionSerializer : IMetadataSectionType<PackMetadataSection>
{
    public string MetadataSectionName => "pack";
    
    public PackMetadataSection FromJson(JsonObject obj)
    {
        var component = Component.FromJson(obj["description"]);
        var version = obj["pack_format"]!.GetValue<int>();
        return new PackMetadataSection(component, version);
    }

    public JsonObject ToJson(PackMetadataSection obj)
    {
        return new JsonObject
        {
            ["description"] = obj.Description.ToJson(),
            ["pack_format"] = obj.PackFormat
        };
    }
}