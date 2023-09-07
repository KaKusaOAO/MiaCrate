namespace MiaCrate.Client;

public class Timer
{
    private readonly float _msPerTick;
    private long _lastMs;

    public Timer(float f, long l)
    {
        _msPerTick = 1000f / f;
        _lastMs = l;
    }

    public int AdvanceTime(long l)
    {
        TickDelta = (l - _lastMs) / _msPerTick;
        _lastMs = l;
        PartialTick += TickDelta;

        var i = (int) PartialTick;
        PartialTick -= i;
        return i;
    }

    public float PartialTick { get; private set; }

    public float TickDelta { get; private set; }
}