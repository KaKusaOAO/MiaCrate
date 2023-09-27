namespace MiaCrate.Server;

public class TickTask : IRunnable
{
    private readonly IRunnable _runnable;

    public int Tick { get; }

    public TickTask(int tick, IRunnable runnable)
    {
        Tick = tick;
        _runnable = runnable;
    }

    public void Run() => _runnable.Run();
}