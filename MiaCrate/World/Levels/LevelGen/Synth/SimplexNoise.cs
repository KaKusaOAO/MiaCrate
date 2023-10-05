namespace MiaCrate.World;

public class SimplexNoise
{
    private static readonly int[][] _gradient = {
        new[] {1, 1, 0}, 
        new[] {-1, 1, 0}, 
        new[] {1, -1, 0}, 
        new[] {-1, -1, 0}, 
        new[] {1, 0, 1}, 
        new[] {-1, 0, 1}, 
        new[] {1, 0, -1}, 
        new[] {-1, 0, -1}, 
        new[] {0, 1, 1}, 
        new[] {0, -1, 1}, 
        new[] {0, 1, -1}, 
        new[] {0, -1, -1}, 
        new[] {1, 1, 0}, 
        new[] {0, -1, 1}, 
        new[] {-1, 1, 0}, 
        new[] {0, -1, -1}
    };

    private static readonly double _sqrt3 = Math.Sqrt(3);
    private static readonly double _f2;
    private static readonly double _g2;
    private readonly int[] _p = new int[512];

    public double Xo { get; }
    public double Yo { get; }
    public double Zo { get; }

    public SimplexNoise(IRandomSource rand)
    {
        Xo = rand.NextDouble() * 256;
        Yo = rand.NextDouble() * 256;
        Zo = rand.NextDouble() * 256;

        for (var i = 0; i < 256; _p[i] = i++)
        {
            // Empty. Fill the array.
        }

        for (var i = 0; i < 256; i++)
        {
            var j = rand.Next(256 - i);
            (_p[i], _p[j + i]) = (_p[j + i], _p[i]);
        }
    }
}