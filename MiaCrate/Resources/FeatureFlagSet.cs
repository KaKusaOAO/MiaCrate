namespace MiaCrate.Resources;

public sealed class FeatureFlagSet
{
    public static readonly FeatureFlagSet Empty = new(null, 0);
    public const int MaxContainerSize = 64;
    private readonly FeatureFlagUniverse? _universe;
    private readonly long _mask;

    private FeatureFlagSet(FeatureFlagUniverse? universe, long mask)
    {
        _universe = universe;
        _mask = mask;
    }

    internal static FeatureFlagSet Create(FeatureFlagUniverse universe, ICollection<FeatureFlag> flags)
    {
        if (!flags.Any()) return Empty;

        var mask = ComputeMask(universe, 0L, flags);
        return new FeatureFlagSet(universe, mask);
    }

    public static FeatureFlagSet Of() => Empty;
    public static FeatureFlagSet Of(FeatureFlag flag) => new(flag.Universe, flag.Mask);
    public static FeatureFlagSet Of(FeatureFlag flag, params FeatureFlag[] remaining)
    {
        var mask = remaining.Length == 0 ? flag.Mask : ComputeMask(flag.Universe, flag.Mask, remaining);
        return new FeatureFlagSet(flag.Universe, mask);
    }

    private static long ComputeMask(FeatureFlagUniverse universe, long initialMask, IEnumerable<FeatureFlag> flags)
    {
        foreach (var flag in flags)
        {
            if (universe != flag.Universe)
                throw new Exception($"Mismatched feature universe, expected {universe} but got {flag.Universe}");

            initialMask |= flag.Mask;
        }

        return initialMask;
    }

    public bool Contains(FeatureFlag flag)
    {
        if (_universe != flag.Universe) return false;
        return (_mask & flag.Mask) != 0;
    }

    public bool IsSubsetOf(FeatureFlagSet set)
    {
        if (_universe == null) return true;
        if (_universe != set._universe) return false;
        return (_mask & ~set._mask) == 0;
    }

    public FeatureFlagSet Join(FeatureFlagSet other)
    {
        if (_universe == null) return other;
        if (other._universe == null) return this;
        
        if (_universe != other._universe)
            throw new ArgumentException($"Mismatched set elements: '{_universe}' != '{other._universe}'");

        return new FeatureFlagSet(_universe, _mask | other._mask);
    }

    public override bool Equals(object obj)
    {
        if (obj is not FeatureFlagSet other) return false;
        return other._universe == _universe && other._mask == _mask;
    }

    public override int GetHashCode()
    {
        var h = _mask * -7046029254386353131L;
        h ^= h >>> 32;
        return (int) (h ^ h >>> 16);
    }
}