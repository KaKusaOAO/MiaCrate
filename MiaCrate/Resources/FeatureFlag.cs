using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using Mochi.Utils;

namespace MiaCrate.Resources;

public class FeatureFlag
{
    internal FeatureFlagUniverse Universe { get; set; }
    internal long Mask { get; set; }

    internal FeatureFlag(FeatureFlagUniverse universe, int bits)
    {
        Universe = universe;
        Mask = 1L << bits;
    }
}

public class FeatureFlagRegistry
{
    private readonly FeatureFlagUniverse _universe;
    private readonly Dictionary<ResourceLocation, FeatureFlag> _names;
    public FeatureFlagSet AllFlags { get; }

    public ICodec<FeatureFlagSet> Codec => ResourceLocation.Codec.ListCodec.CoSelectSelectMany(
        list =>
        {
            var set = new HashSet<ResourceLocation>();
            var features = FromNames(list, x => set.Add(x));

            return set.Any()
                ? DataResult.Error<FeatureFlagSet>(() => $"Unknown feature ids: {set}")
                : DataResult.Success(features);
        },
        features => ToNames(features).ToList()
    );

    public FeatureFlagRegistry(FeatureFlagUniverse universe, FeatureFlagSet allFlags,
        Dictionary<ResourceLocation, FeatureFlag> names)
    {
        _universe = universe;
        AllFlags = allFlags;
        _names = names;
    }

    public bool IsSubset(FeatureFlagSet set) => set.IsSubsetOf(AllFlags);

    public FeatureFlagSet FromNames(IEnumerable<ResourceLocation> locations)
    {
        return FromNames(locations, l =>
        {
            Logger.Warn($"Unknown feature flag: {l}");
        });
    }

    public FeatureFlagSet Subset(params FeatureFlag[] flags) => FeatureFlagSet.Create(_universe, flags);

    public HashSet<ResourceLocation> ToNames(FeatureFlagSet set)
    {
        var result = new HashSet<ResourceLocation>();
        foreach (var (key, value) in _names)
        {
            if (set.Contains(value))
            {
                result.Add(key);
            }
        }

        return result;
    }

    public FeatureFlagSet FromNames(IEnumerable<ResourceLocation> locations, Action<ResourceLocation> handle)
    {
        var set = new HashSet<FeatureFlag>();
        foreach (var location in locations)
        {
            var flag = _names.GetValueOrDefault(location);
            
            if (flag == null)
            {
                handle(location);
            }
            else
            {
                set.Add(flag);
            }
        }

        return FeatureFlagSet.Create(_universe, set);
    }

    public class Builder
    {
        private readonly FeatureFlagUniverse _universe;
        private readonly Dictionary<ResourceLocation, FeatureFlag> _flags = new();
        private int _id;

        public Builder(string id)
        {
            _universe = new FeatureFlagUniverse(id);
        }

        public FeatureFlag CreateVanilla(string str) =>
            Create(new ResourceLocation(ResourceLocation.DefaultNamespace, str));

        public FeatureFlag Create(ResourceLocation location)
        {
            if (_id >= 64) 
                throw new Exception("Too many feature flags");
            
            if (_flags.ContainsKey(location))
                throw new Exception($"Duplicate feature flag {location}");

            var flag = new FeatureFlag(_universe, _id++);
            _flags[location] = flag;
            return flag;
        }

        public FeatureFlagRegistry Build()
        {
            var set = FeatureFlagSet.Create(_universe, _flags.Values);
            return new FeatureFlagRegistry(_universe, set, _flags);
        }
    }
}