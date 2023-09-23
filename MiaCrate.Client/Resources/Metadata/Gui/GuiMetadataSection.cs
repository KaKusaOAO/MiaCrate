using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Resources;

namespace MiaCrate.Client.Resources;

public record GuiMetadataSection(IGuiSpriteScaling Scaling)
{
    public static GuiMetadataSection Default { get; } = new(IGuiSpriteScaling.Default);

    public static ICodec<GuiMetadataSection> Codec { get; } =
        RecordCodecBuilder.Create<GuiMetadataSection>(instance => instance
            .Group(
                ExtraCodecs.StrictOptionalField(IGuiSpriteScaling.Codec, "scaling", IGuiSpriteScaling.Default)
                    .ForGetter<GuiMetadataSection>(s => s.Scaling)
            )
            .Apply(instance, s => new GuiMetadataSection(s))
        );
    
    public static IMetadataSectionType<GuiMetadataSection> Type { get; } = 
        IMetadataSectionType<GuiMetadataSection>.FromCodec("gui", Codec);
}