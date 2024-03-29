﻿namespace MiaCrate.Resources;

public interface IPackResources : IDisposable
{
    public Func<Stream>? GetResource(PackType type, ResourceLocation location);
    public Func<Stream>? GetRootResource(params string[] str);
    public void ListResources(PackType type, string str, string path, ResourceOutputDelegate output);
    public string PackId { get; }
    public bool IsBuiltin => false;
    public ISet<string> GetNamespaces(PackType type);
    public T? GetMetadataSection<T>(IMetadataSectionSerializer<T> serializer) where T : class;
    public delegate void ResourceOutputDelegate(ResourceLocation location, Func<Stream> stream);
}