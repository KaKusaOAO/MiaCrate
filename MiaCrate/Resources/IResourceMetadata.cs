using System.Text.Json.Nodes;
using Mochi.Utils;

namespace MiaCrate.Resources;

public interface IResourceMetadata
{
    public static readonly IResourceMetadata Empty = new EmptyMetadata();
    public static readonly Func<IResourceMetadata> EmptySupplier = () => Empty;

    public static IResourceMetadata FromJsonStream(Stream stream)
    {
        var metadata = JsonNode.Parse(stream)!.AsObject();
        return new JsonMetadata(metadata);
    }
    
    public IOptional GetSection(IMetadataSectionSerializer serializer);

    public IOptional<T> GetSection<T>(IMetadataSectionSerializer<T> serializer) =>
        GetSection((IMetadataSectionSerializer) serializer)
            .Select(e => (T) e);

    public IResourceMetadata CopySections(IEnumerable<IMetadataSectionSerializer> serializers)
    {
        var builder = new Builder();
        foreach (var serializer in serializers)
        {
            CopySection(builder, serializer);
        }

        return builder.Build();
    }

    private void CopySection(Builder builder, IMetadataSectionSerializer serializer)
    {
        var optional = GetSection(serializer);
        if (optional.IsEmpty) return;
        builder.Put(serializer, optional.Value);
    }

    private class JsonMetadata : IResourceMetadata
    {
        private readonly JsonObject _metadata;

        public JsonMetadata(JsonObject metadata)
        {
            _metadata = metadata;
        }

        public IOptional GetSection(IMetadataSectionSerializer serializer)
        {
            var name = serializer.MetadataSectionName;
            return _metadata.TryGetPropertyValue(name, out var node)
                ? Optional.Of(serializer.FromJson(node!.AsObject()))
                : Optional.Empty<object>();
        }
    }
    
    private class EmptyMetadata : IResourceMetadata
    {
        public IOptional GetSection(IMetadataSectionSerializer serializer) => Optional.Empty<object>();
    }

    public class Builder
    {
        private readonly Dictionary<IMetadataSectionSerializer, object> _dict = new();

        public Builder Put<T>(IMetadataSectionSerializer<T> serializer, T obj)
        {
            _dict[serializer] = obj!;
            return this;
        }
        
        public Builder Put(IMetadataSectionSerializer serializer, object obj)
        {
            _dict[serializer] = obj;
            return this;
        }

        public IResourceMetadata Build() => new Instance(this);

        private class Instance : IResourceMetadata
        {
            private readonly Builder _builder;

            public Instance(Builder builder)
            {
                _builder = builder;
            }

            public IOptional GetSection(IMetadataSectionSerializer serializer) =>
                Optional.OfNullable(_builder._dict.GetValueOrDefault(serializer));
        }
    }
}