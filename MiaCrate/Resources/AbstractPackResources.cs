using System.Text.Json;
using System.Text.Json.Nodes;
using Mochi.Utils;

namespace MiaCrate.Resources;

public abstract class AbstractPackResources : IPackResources
{

    public string PackId { get; }
    public bool IsBuiltIn { get; }
    
    protected AbstractPackResources(string name, bool isBuiltIn)
    {
        PackId = name;
        IsBuiltIn = isBuiltIn;
    }

    public static T? GetMetadataFromStream<T>(IMetadataSectionSerializer<T> serializer, Stream stream) where T : class
    {
        JsonObject obj;
        try
        {
            obj = JsonNode.Parse(stream)!.AsObject();
        }
        catch (Exception ex)
        {
            Logger.Error($"Couldn't load {serializer.MetadataSectionName} metadata");
            Logger.Error(ex);
            return null;
        }

        if (!obj.ContainsKey(serializer.MetadataSectionName)) return null;

        try
        {
            return serializer.FromJson(obj[serializer.MetadataSectionName]!.AsObject());
        }
        catch (Exception ex)
        {
            Logger.Error($"Couldn't load {serializer.MetadataSectionName} metadata");
            Logger.Error(ex);
            return null;
        }
    }

    public T? GetMetadataSection<T>(IMetadataSectionSerializer<T> serializer) where T : class
    {
        var supplier = GetRootResource("pack.mcmeta");
        if (supplier == null) return null;

        using var stream = supplier();
        return GetMetadataFromStream(serializer, stream);
    }

    public abstract void Dispose();

    public Func<Stream>? GetResource(PackType type, ResourceLocation location)
    {
        throw new NotImplementedException();
    }

    public Func<Stream>? GetRootResource(params string[] str)
    {
        throw new NotImplementedException();
    }

    public void ListResources(PackType type, string str, string path, IPackResources.ResourceOutputDelegate output)
    {
        throw new NotImplementedException();
    }

    public ISet<string> GetNamespaces(PackType type)
    {
        throw new NotImplementedException();
    }
}