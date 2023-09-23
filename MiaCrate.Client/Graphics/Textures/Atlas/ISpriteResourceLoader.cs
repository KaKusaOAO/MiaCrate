using MiaCrate.Client.Resources;
using MiaCrate.Resources;
using Mochi.Utils;

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
            IResourceMetadata metadata;
            try
            {
                metadata = resource.Metadata.CopySections(_serializers);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to parse metadata from {location}");
                Logger.Error(ex);
                return null;
            }

            NativeImage image;
            try
            {
                using var stream = resource.Open();
                image = NativeImage.Read(stream);
            }
            catch (Exception ex)
            {
                Logger.Error($"Using missing texture, unable to load {location}");
                Logger.Error(ex);
                return null;
            }

            var section = metadata.GetSection(AnimationMetadataSection.Serializer)
                .OrElse(AnimationMetadataSection.Empty);
            var frameSize = section.CalculateFrameSize(image.Width, image.Height);
            if (image.Width % frameSize.Width == 0 && image.Height % frameSize.Height == 0)
            {
                return new SpriteContents(location, frameSize, image, metadata);
            }
            
            Logger.Error($"Image {location} size {image.Width},{image.Height} is not multiple of frame size {frameSize.Width},{frameSize.Height}");
            image.Dispose();
            return null;
        }
    }
}