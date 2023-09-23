using System.Text.Json.Nodes;

namespace MiaCrate.Resources;

public interface IMetadataSectionSerializer
{
    public string MetadataSectionName { get; }
    public object FromJson(JsonObject obj);
}

public interface IMetadataSectionSerializer<out T> : IMetadataSectionSerializer
{
    public new T FromJson(JsonObject obj);
    object IMetadataSectionSerializer.FromJson(JsonObject obj) => FromJson(obj)!;
}