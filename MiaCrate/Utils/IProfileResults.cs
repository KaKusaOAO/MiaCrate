namespace MiaCrate;

public interface IProfileResults
{
    public bool SaveResults(string path);
    public long StartTimeNano { get; }
    public int StartTimeTicks { get; }
    public long EndTimeNano { get; }
    public int EndTimeTicks { get; }

    public long NanoDuration => EndTimeNano - StartTimeNano;
    public int TickDuration => EndTimeTicks - StartTimeTicks;
}