using System.Text.Json.Nodes;

namespace MiaCrate.Resources;

public interface IMetadataSectionSerializer
{
    public string MetadataSectionName { get; }
}

public interface IMetadataSectionSerializer<out T> : IMetadataSectionSerializer
{
    public T FromJson(JsonObject obj);
}