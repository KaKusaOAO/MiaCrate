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

    public ActiveProfiler.PathEntry? GetEntry(string str) => null;
}