namespace MiaCrate;

public interface IBitRandomSource : IRandomSource
{
    public const float SingleMultiplier = 5.9604645e-8f;
    public const double DoubleMultiplier = 1.1102230246251565e-16;
    
    public int NextBits(int bits);

    int IRandomSource.Next() => NextBits(32);

    int IRandomSource.Next(int bound)
    {
        if (bound <= 0)
            throw new ArgumentException("Bound must be positive");

        if ((bound & bound - 1) == 0)
            return (int) (bound * (long) NextBits(31) >> 31);

        int j, k;
        do
        {
            j = NextBits(31);
            k = j % bound;
        } while (j - k + (bound - 1) < 0);

        return k;
    }

    long IRandomSource.NextInt64()
    {
        var i = NextBits(32);
        var j = NextBits(32);
        var l = (long)i << 32;
        return l + j;
    }

    float IRandomSource.NextSingle() => NextBits(24) * SingleMultiplier;

    bool IRandomSource.NextBool() => NextBits(1) != 0;

    double IRandomSource.NextDouble()
    {
        var i = NextBits(26);
        var j = NextBits(27);
        var l = ((long) i << 27) + j;
        return l * DoubleMultiplier;
    }
}