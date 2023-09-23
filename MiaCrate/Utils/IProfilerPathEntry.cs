namespace MiaCrate;

public interface IProfilerPathEntry
{
    public long Duration { get; }
    public long MaxDuration { get; }
    public long Count { get; }
    public Dictionary<string, long> Counters { get; }
}