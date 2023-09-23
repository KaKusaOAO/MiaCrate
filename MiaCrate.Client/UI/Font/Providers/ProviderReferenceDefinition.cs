using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Client.UI;

public record ProviderReferenceDefinition(ResourceLocation Id) : IGlyphProviderDefinition
{
    public static IMapCodec<ProviderReferenceDefinition> Codec { get; } =
        RecordCodecBuilder.MapCodec<ProviderReferenceDefinition>(instance => instance
            .Group(
                ResourceLocation.Codec.FieldOf("id").ForGetter<ProviderReferenceDefinition>(d => d.Id)
            )
            .Apply(instance, r => new ProviderReferenceDefinition(r))
        );

    public GlyphProviderType Type => GlyphProviderType.Reference;

    public IEither<IGlyphProviderDefinition.ILoader, IGlyphProviderDefinition.Reference> Unpack() =>
        Either.CreateRight(new IGlyphProviderDefinition.Reference(Id))
            .Left<IGlyphProviderDefinition.ILoader>();
}