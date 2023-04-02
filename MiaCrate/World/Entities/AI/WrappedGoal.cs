namespace MiaCrate.World.Entities.AI;

public class WrappedGoal : Goal
{
    public int Priority { get; }
    public Goal Goal { get; }
    public virtual bool IsRunning { get; private set; }

    public WrappedGoal(int priority, Goal goal)
    {
        Priority = priority;
        Goal = goal;
    }
    
    public override bool CanUse => Goal.CanUse;
    public override bool CanContinueToUse => Goal.CanContinueToUse;
    public override bool IsInterruptable => Goal.IsInterruptable;
    public override bool RequiresUpdateEveryTick => Goal.RequiresUpdateEveryTick;

    public override GoalFlag Flags
    {
        get => Goal.Flags;
        set => Goal.Flags = value;
    }

    public override void Start()
    {
        if (IsRunning) return;
        
        Goal.Start();
        IsRunning = true;
    }

    public override void Tick() => Goal.Tick();
    
    public override void Stop()
    {
        if (!IsRunning) return;
        
        Goal.Stop();
        IsRunning = false;
    }
}