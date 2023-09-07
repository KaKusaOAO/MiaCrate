namespace MiaCrate;

public class LegacyRandomSource : IBitRandomSource
{
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
        if (Interlocked.CompareExchange(ref _seed, (seed ^ 25214903917L) & 281474976710655L, expected) != expected) 
            throw new Exception("?");
        _gaussianSource.Reset();
    }

    public double NextGaussian() => _gaussianSource.NextGaussian();

    public int NextBits(int bits)
    {
        var seed = Interlocked.Read(ref _seed);
        var val = (seed ^ 25214903917L) & 281474976710655L;
        if (Interlocked.CompareExchange(ref _seed, val, seed) != seed) 
            throw new Exception("?");
        
        return (int) (val >> 48 - bits);
    }
}