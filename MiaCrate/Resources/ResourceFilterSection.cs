using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Resources;

public class ResourceFilterSection
{
    private static readonly ICodec<ResourceFilterSection> _codec =
        RecordCodecBuilder.Create<ResourceFilterSection>(instance => instance
            .Group(
                ResourceLocationPattern.Codec.ListCodec.FieldOf("block")
                    .ForGetter<ResourceFilterSection>(s => s._blockList)
            ).Apply(instance, l => new ResourceFilterSection(l))
        );

    public static readonly IMetadataSectionType<ResourceFilterSection> Type = 
        MetadataSectionType.FromCodec("filter", _codec);

    private readonly List<ResourceLocationPattern> _blockList;

    public ResourceFilterSection(List<ResourceLocationPattern> blockList)
    {
        _blockList = new List<ResourceLocationPattern>(blockList);
    }

    public bool IsNamespaceFiltered(string ns) => _blockList.Any(p => p.TestNamespace(ns));
    public bool IsPathFiltered(string path) => _blockList.Any(p => p.TestPath(path));
}