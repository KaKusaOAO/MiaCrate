using MiaCrate.Client.UI;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Client.Fonts;

public class SpaceProvider : IGlyphProvider
{
    private readonly Dictionary<int, IGlyphInfo> _glyphs;

    public SpaceProvider(Dictionary<int, float> dict)
    {
        _glyphs = new Dictionary<int, IGlyphInfo>();
        foreach (var (key, value) in dict)
        {
            _glyphs[key] = IGlyphInfo.ISpace.Create(value);
        }
    }

    public ISet<int> GetSupportedGlyphs() => _glyphs.Keys.ToHashSet();

    public record Definition(Dictionary<int, float> Advances) : IGlyphProviderDefinition
    {
        public static IMapCodec<Definition> Codec { get; } =
            RecordCodecBuilder.MapCodec<Definition>(instance => instance
                .Group(
                    Data.Codec.UnboundedMap(ExtraCodecs.Codepoint, Data.Codec.Float)
                        .FieldOf("advances")
                        .ForGetter<Definition>(f => f.Advances)
                )
                .Apply(instance, d => new Definition(d))
            );

        public GlyphProviderType Type => GlyphProviderType.Space;

        public IEither<IGlyphProviderDefinition.ILoader, IGlyphProviderDefinition.Reference> Unpack()
        {
            var loader = IGlyphProviderDefinition.ILoader.Create(_ => new SpaceProvider(Advances));
            return Either.CreateLeft(loader)
                .Right<IGlyphProviderDefinition.Reference>();
        }
    }
}