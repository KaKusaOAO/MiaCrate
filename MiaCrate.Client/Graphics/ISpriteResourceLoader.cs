using MiaCrate.Resources;

namespace MiaCrate.Client.Graphics;

public interface ISpriteResourceLoader
{
    public SpriteContents? LoadSprite(ResourceLocation location, Resource resource);

    public static ISpriteResourceLoader Create(IEnumerable<IMetadataSectionSerializer> serializers) =>
        new Instance(serializers);

    private class Instance : ISpriteResourceLoader
    {
        private readonly IEnumerable<IMetadataSectionSerializer> _serializers;

        public Instance(IEnumerable<IMetadataSectionSerializer> serializers)
        {
            _serializers = serializers;
        }

        public SpriteContents? LoadSprite(ResourceLocation location, Resource resource)
        {
            throw new NotImplementedException();
        }
    }
}