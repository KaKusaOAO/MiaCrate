using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Resources;

public static class FeatureFlags
{
    public static readonly FeatureFlag Vanilla;
    public static readonly FeatureFlag Bundle;
    public static readonly FeatureFlagRegistry Registry;
    public static readonly ICodec<FeatureFlagSet> Codec;
    public static readonly FeatureFlagSet VanillaSet;
    public static readonly FeatureFlagSet DefaultFlags;

    static FeatureFlags()
    {
        var builder = new FeatureFlagRegistry.Builder("main");

        // Create all the flags we have
        Vanilla = builder.CreateVanilla("vanilla");
        Bundle = builder.CreateVanilla("bundle");
        
        // Then we build the flags registry
        Registry = builder.Build();
        Codec = Registry.Codec;
        
        VanillaSet = FeatureFlagSet.Of(Vanilla);
        DefaultFlags = VanillaSet;
    }
}