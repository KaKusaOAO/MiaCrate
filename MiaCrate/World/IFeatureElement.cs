using MiaCrate.Resources;

namespace MiaCrate.World;

public interface IFeatureElement
{
    FeatureFlagSet RequiredFeatures { get; }

    bool IsEnabled(FeatureFlagSet set) => RequiredFeatures.IsSubsetOf(set);
}

public static class FeatureElementExtension
{
    public static bool IsEnabled(this IFeatureElement element, FeatureFlagSet set) => element.IsEnabled(set);
}