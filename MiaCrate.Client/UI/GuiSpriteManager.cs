using MiaCrate.Client.Graphics;
using MiaCrate.Client.Resources;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.UI;

public class GuiSpriteManager : TextureAtlasHolder
{
    private static readonly List<IMetadataSectionSerializer> _metadataSections = new()
    {
        AnimationMetadataSection.Serializer,
        GuiMetadataSection.Type
    };

    public GuiSpriteManager(TextureManager manager)
        : base(manager, new ResourceLocation("textures/atlas/gui.png"), new ResourceLocation("gui"), _metadataSections)
    {

    }

    public IGuiSpriteScaling GetSpriteScaling(TextureAtlasSprite sprite) => GetMetadata(sprite).Scaling;

    private GuiMetadataSection GetMetadata(TextureAtlasSprite sprite)
    {
        return sprite.Contents.Metadata
            .GetSection(GuiMetadataSection.Type)
            .OrElse(GuiMetadataSection.Default);
    }
}