namespace Mochi.World.Entities.AI;

internal class EmptyWrappedGoal : WrappedGoal
{
    private class EmptyGoal : Goal
    {
        public override bool CanUse => false;
    }
    
    public EmptyWrappedGoal() : base(int.MaxValue, new EmptyGoal())
    {
    }

    public override bool IsRunning => false;
}