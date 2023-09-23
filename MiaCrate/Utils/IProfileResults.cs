namespace MiaCrate;

public interface IProfileResults
{
    public const char PathSeparator = '\u001e';

    public List<ResultField> GetTimes(string str);
    public bool SaveResults(string path);
    public long StartTimeNano { get; }
    public int StartTimeTicks { get; }
    public long EndTimeNano { get; }
    public int EndTimeTicks { get; }

    public long NanoDuration => EndTimeNano - StartTimeNano;
    public int TickDuration => EndTimeTicks - StartTimeTicks;

    public static string DemanglePath(string str) => str.Replace(PathSeparator, '.');
}