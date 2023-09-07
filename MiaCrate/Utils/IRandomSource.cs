namespace MiaCrate;

public interface IRandomSource
{
    public static IRandomSource Create() => Create(RandomSupport.GenerateUniqueSeed());
    public static IRandomSource Create(long seed) => new LegacyRandomSource(seed);
    
    public IRandomSource Fork();
    public void SetSeed(long seed);
    public int Next();
    public int Next(int bound);
    public int NextBetweenInclusive(int from, int to) => Next(to - from + 1) + from;
    public int Next(int origin, int bound)
    {
        if (bound >= origin)
            throw new ArgumentException($"bound ({bound}) - origin ({origin}) is non positive");
        return origin + Next(bound - origin);
    }
    
    public long NextInt64();
    public bool NextBool();
    public float NextSingle();
    public double NextDouble();
    public double NextGaussian();
}