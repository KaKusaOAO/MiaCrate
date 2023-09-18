using Mochi.Utils;

namespace MiaCrate.Resources;

public interface IResourceMetadata
{
    public static readonly IResourceMetadata Empty = new EmptyMetadata();
    public static readonly Func<IResourceMetadata> EmptySupplier = () => Empty;
    
    public IOptional<T> GetSection<T>(IMetadataSectionSerializer<T> serializer);

    private class EmptyMetadata : IResourceMetadata
    {
        public IOptional<T> GetSection<T>(IMetadataSectionSerializer<T> serializer) => Optional.Empty<T>();
    }

    public class Builder
    {
        private readonly Dictionary<IMetadataSectionSerializer, object> _dict = new();

        public Builder Put<T>(IMetadataSectionSerializer<T> serializer, T obj)
        {
            _dict[serializer] = obj!;
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

            public IOptional<T> GetSection<T>(IMetadataSectionSerializer<T> serializer) => 
                Optional.OfNullable(_builder._dict.GetValueOrDefault(serializer)).Select(r => (T) r);
        }
    }
}