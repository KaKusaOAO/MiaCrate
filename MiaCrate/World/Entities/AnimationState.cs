namespace MiaCrate.World.Entities;

public class AnimationState
{
    private const long Stopped = long.MaxValue;
    
    private long _lastTime = Stopped;
    
    public long AccumulatedTime { get; private set; }
    public bool IsStarted => _lastTime != Stopped;
    
    public void Start(int i)
    {
        _lastTime = i * 1000L / 20L;
        AccumulatedTime = 0;
    }

    public void StartIfStopped(int i)
    {
        if (IsStarted) return;
        Start(i);
    }

    public void AnimateWhen(bool bl, int i)
    {
        if (bl) StartIfStopped(i);
        else Stop();
    }

    public void Stop()
    {
        _lastTime = Stopped;
    }

    public void IfStarted(Action<AnimationState> consumer)
    {
        if (!IsStarted) return;
        consumer(this);
    }

    public void UpdateTime(float f, float g)
    {
        if (!IsStarted) return;

        var l = Util.LFloor(f * 1000 / 20);
        AccumulatedTime += (long) ((l - _lastTime) * g);
        _lastTime = l;
    }
}