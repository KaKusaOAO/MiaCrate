namespace MiaCrate;

public class EmptyProfileResults : IProfileResults
{
    public static EmptyProfileResults Instance { get; } = new();
    private EmptyProfileResults() {}

    public List<ResultField> GetTimes(string str) => new();
    public bool SaveResults(string path) => false;
    public long StartTimeNano => 0;
    public int StartTimeTicks => 0;
    public long EndTimeNano => 0;
    public int EndTimeTicks => 0;
}