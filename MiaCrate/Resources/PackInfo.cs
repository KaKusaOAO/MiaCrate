using Mochi.Texts;

namespace MiaCrate.Resources;

public class PackInfo
{
    public PackInfo(IComponent description, int format, FeatureFlagSet requestedFeatures)
    {
        Description = description;
        Format = format;
        RequestedFeatures = requestedFeatures;
    }

    public IComponent Description { get; }
    public int Format { get; }
    public FeatureFlagSet RequestedFeatures { get; }

    public PackCompatibility GetCompatibility(PackType type) => PackCompatibility.ForFormat(Format, type);
}