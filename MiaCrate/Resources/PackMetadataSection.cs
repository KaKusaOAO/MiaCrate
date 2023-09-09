using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.Resources;

public class PackMetadataSection
{
    public static readonly ICodec<PackMetadataSection> Codec = RecordCodecBuilder
        .Create<PackMetadataSection>(instance => instance
            .Group(
                ExtraCodecs.Component
                    .FieldOf("description")
                    .ForGetter<PackMetadataSection>(s => s.Description),
                Data.Codec.Int
                    .FieldOf("pack_format")
                    .ForGetter<PackMetadataSection>(s => s.PackFormat),
                InclusiveRange.Codec(Data.Codec.Int)
                    .OptionalFieldOf("supported_formats")
                    .ForGetter<PackMetadataSection>(s => s.SupportedFormats)
            )
            .Apply(instance, (d, format, supported) => 
                new PackMetadataSection(d, format, supported)
            )
        );
    
    public static readonly IMetadataSectionType<PackMetadataSection> Type = 
        IMetadataSectionType<PackMetadataSection>.FromCodec("pack", Codec);
    
    public IComponent Description { get; }
    public int PackFormat { get; }
    public IOptional<InclusiveRange<int>> SupportedFormats { get; }

    public PackMetadataSection(IComponent description, int packFormat)
        : this(description, packFormat, Optional.Empty<InclusiveRange<int>>()) {}
    
    public PackMetadataSection(IComponent description, int packFormat, InclusiveRange<int> supportedFormats)
        : this(description, packFormat, Optional.Of(supportedFormats)) {}
    
    public PackMetadataSection(IComponent description, int packFormat, IOptional<InclusiveRange<int>> supportedFormats)
    {
        Description = description;
        PackFormat = packFormat;
        SupportedFormats = supportedFormats;
    }
}