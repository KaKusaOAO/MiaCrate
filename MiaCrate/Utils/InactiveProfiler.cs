namespace MiaCrate;

public class InactiveProfiler : IProfileCollector
{
    public static readonly InactiveProfiler Instance = new();
    
    private InactiveProfiler()
    {
        
    }

    public void StartTick()
    {
        
    }

    public void EndTick()
    {
        
    }

    public void Push(string str)
    {
        
    }

    public void Push(Func<string> func)
    {
        
    }

    public void Pop()
    {
        
    }

    public void PopPush(string str)
    {
        
    }

    public void PopPush(Func<string> func)
    {
        
    }

    public void IncrementCounter(Func<string> func, int count = 1)
    {
        
    }

    public void IncrementCounter(string str, int count = 1)
    {
        
    }

    public IProfileResults Results => EmptyProfileResults.Instance;
}

public class EmptyProfileResults : IProfileResults
{
    public static readonly EmptyProfileResults Instance = new();
    
    private EmptyProfileResults() {}
    
    public bool SaveResults(string path) => false;
    public long StartTimeNano => 0;
    public int StartTimeTicks => 0;
    public long EndTimeNano => 0;
    public int EndTimeTicks => 0;
}