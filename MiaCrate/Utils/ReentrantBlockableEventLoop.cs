using MiaCrate.Common;

namespace MiaCrate;

public abstract class ReentrantBlockableEventLoop<T> : BlockableEventLoop<T> where T : IRunnable
{
    private int _reentrantCount;
    
    protected ReentrantBlockableEventLoop(string name) : base(name) { }

    protected override bool ScheduleExecutables() => IsRunningTask || base.ScheduleExecutables();

    protected bool IsRunningTask => _reentrantCount != 0;

    protected override void DoRunTask(T runnable)
    {
        ++_reentrantCount;
        
        try
        {
            base.DoRunTask(runnable);
        }
        finally
        {
            --_reentrantCount;
        }
    }
}