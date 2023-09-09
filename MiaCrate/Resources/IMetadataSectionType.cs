using System.ComponentModel;
using System.Text.Json.Nodes;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Resources;

public static class MetadataSectionType
{
    public static IMetadataSectionType<T> FromCodec<T>(string name, ICodec<T> codec) =>
        IMetadataSectionType<T>.FromCodec(name, codec);
}

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