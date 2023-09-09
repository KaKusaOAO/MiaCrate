using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Resources;

public class FeatureFlagsMetadataSection
{
    public FeatureFlagSet Flags { get; }

    private static readonly ICodec<FeatureFlagsMetadataSection> _codec =
        RecordCodecBuilder.Create<FeatureFlagsMetadataSection>(instance =>
            instance.Group(
                FeatureFlags.Codec.FieldOf("enabled").ForGetter<FeatureFlagsMetadataSection>(section => section.Flags)
            ).Apply(instance, x => new FeatureFlagsMetadataSection(x))
        );

    public static readonly IMetadataSectionType<FeatureFlagsMetadataSection> Type = 
        MetadataSectionType.FromCodec("features", _codec);

    public FeatureFlagsMetadataSection(FeatureFlagSet flags)
    {
        Flags = flags;
    }
}

