namespace MiaCrate;

public class LegacyRandomSource : IBitRandomSource
{
    private const int ModulusBits = 48;
    private const long ModulusMask = 281474976710655L;
    private const long Multiplier = 25214903917L;
    private const long Increment = 11;
    
    private long _seed;
    private readonly MarsagliaPolarGaussian _gaussianSource;
    private readonly SemaphoreSlim _seedLock = new(1, 1);
    
    public LegacyRandomSource(long seed)
    {
        _gaussianSource = new MarsagliaPolarGaussian(this);
        SetSeed(seed);
    }

    private IBitRandomSource Boxed() => this;

    public IRandomSource Fork() => new LegacyRandomSource(Boxed().NextInt64());

    public void SetSeed(long seed)
    {
        var expected = Interlocked.Read(ref _seed);
        if (Interlocked.CompareExchange(ref _seed, (seed ^ Multiplier) & ModulusMask, expected) != expected) 
            throw new Exception("?");
        _gaussianSource.Reset();
    }

    public double NextGaussian() => _gaussianSource.NextGaussian();

    public int NextBits(int bits)
    {
        var seed = Interlocked.Read(ref _seed);
        var val = (seed * Multiplier + Increment) & ModulusMask;
        if (Interlocked.CompareExchange(ref _seed, val, seed) != seed) 
            throw new Exception("?");
        
        return (int) (val >> ModulusBits - bits);
    }
}