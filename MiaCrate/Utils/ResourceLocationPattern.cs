using System.Text.RegularExpressions;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using Mochi.Utils;

namespace MiaCrate;

public class ResourceLocationPattern
{
    public static readonly ICodec<ResourceLocationPattern> Codec =
        RecordCodecBuilder.Create<ResourceLocationPattern>(instance => instance
            .Group(
                ExtraCodecs.Regex.OptionalFieldOf("namespace")
                    .ForGetter<ResourceLocationPattern>(p => p._namespaceRegex),
                ExtraCodecs.Regex.OptionalFieldOf("path")
                    .ForGetter<ResourceLocationPattern>(p => p._pathRegex)
            )
            .Apply(instance, (ns, path) => new ResourceLocationPattern(ns, path))
        );
    
    private readonly IOptional<Regex> _namespaceRegex;
    private readonly IOptional<Regex> _pathRegex;
    
    public Predicate<string> NamespacePredicate { get; }
    public Predicate<string> PathPredicate { get; }
    public Predicate<ResourceLocation> LocationPredicate { get; }

    private ResourceLocationPattern(IOptional<Regex> namespaceRegex, IOptional<Regex> pathRegex)
    {
        _namespaceRegex = namespaceRegex;
        NamespacePredicate = namespaceRegex
            .Select<Regex, Predicate<string>>(r => r.IsMatch)
            .OrElse(_ => true);

        _pathRegex = pathRegex;
        PathPredicate = pathRegex
            .Select<Regex, Predicate<string>>(r => r.IsMatch)
            .OrElse(_ => true);

        LocationPredicate = location => NamespacePredicate(location.Namespace) && PathPredicate(location.Path);
    }

    public bool TestNamespace(string ns) => NamespacePredicate(ns);
    public bool TestPath(string path) => PathPredicate(path);
    public bool TestLocation(ResourceLocation location) => LocationPredicate(location);
}