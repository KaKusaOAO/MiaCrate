using Mochi.Utils;

namespace MiaCrate;

public class MarsagliaPolarGaussian
{
    private readonly IRandomSource _randomSource;
    private double _nextNextGaussian;
    private bool _haveNextNextGaussian;

    public MarsagliaPolarGaussian(IRandomSource randomSource)
    {
        _randomSource = randomSource;
    }

    public void Reset()
    {
        _haveNextNextGaussian = false;
    }

    public double NextGaussian()
    {
        if (_haveNextNextGaussian)
        {
            _haveNextNextGaussian = false;
            return _nextNextGaussian;
        }

        double d, e, f;
        do
        {
            do
            {
                d = 2.0 * _randomSource.NextDouble() - 1.0;
                e = 2.0 * _randomSource.NextDouble() - 1.0;
                f = Math.Pow(d, 2) + Math.Pow(e, 2);
            } while (f >= 1.0);
        } while (f == 0.0);

        var g = Math.Sqrt(-2.0 * Math.Log(f) / f);
        _nextNextGaussian = e * g;
        _haveNextNextGaussian = true;
        return d * g;
    }
}