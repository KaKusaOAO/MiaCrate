using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class SimpleTexture : AbstractTexture
{
    protected ResourceLocation Location { get; }

    public SimpleTexture(ResourceLocation location)
    {
        Location = location;
    }
    
    public override void Load(IResourceManager manager)
    {
        var textureImage = GetTextureImage(manager);
        textureImage.ThrowIfError();

        var metadata = textureImage.Metadata;
        var blur = metadata?.IsBlur ?? false;
        var clamp = metadata?.IsClamp ?? false;

        var image = textureImage.Image;
        if (!RenderSystem.IsOnRenderThreadOrInit)
        {
            RenderSystem.RecordRenderCall(() => DoLoad(image, blur, clamp));
        }
        else
        {
            DoLoad(image, blur, clamp);
        }
    }

    private void DoLoad(NativeImage image, bool blur, bool clamp)
    {
        TextureUtil.PrepareImage(Id, 0, image.Width, image.Height);
        image.Upload(0, 0, 0, 0, 0, image.Width, image.Height, blur, clamp, false, true);
    }

    protected virtual TextureImage GetTextureImage(IResourceManager manager) => 
        TextureImage.Load(manager, Location);

    protected class TextureImage : IDisposable
    {
        private readonly NativeImage? _image;
        public TextureMetadataSection? Metadata { get; }

        public NativeImage Image
        {
            get
            {
                ThrowIfError();
                return _image!;
            }
        }

        public Exception? Exception { get; }

        public static TextureImage Load(IResourceManager manager, ResourceLocation location)
        {
            try
            {
                var resource = manager.GetResourceOrThrow(location);
                using var stream = resource.Open();
                var image = NativeImage.Read(stream);

                TextureMetadataSection? metadata = null;
                try
                {
                    metadata = resource.Metadata.GetSection(TextureMetadataSection.Serializer).OrElse(() => null!);
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed reading metadata of: {location}");
                    Logger.Warn(ex);
                }

                return new TextureImage(metadata, image);
            }
            catch (Exception ex)
            {
                return new TextureImage(ex);
            }
        }

        public TextureImage(Exception ex)
        {
            Exception = ex;
        }

        public TextureImage(TextureMetadataSection? metadata, NativeImage image)
        {
            Metadata = metadata;
            _image = image;
        }

        public void Dispose()
        {
            _image?.Dispose();
        }

        public void ThrowIfError()
        {
            if (Exception != null) throw Exception;
        }
    }
}