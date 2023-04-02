namespace MiaCrate.World.Entities.AI;

public abstract class Goal
{
    public virtual GoalFlag Flags { get; set; } = GoalFlag.None;
    public abstract bool CanUse { get; }
    public virtual bool CanContinueToUse => CanUse;
    public virtual bool IsInterruptable => true;
    public virtual bool RequiresUpdateEveryTick => false;
    public virtual void Start() { }
    public virtual void Tick() { }
    public virtual void Stop() { }
}