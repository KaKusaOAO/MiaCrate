namespace Mochi.World.Entities.AI.Goals;

public class FloatGoal : Goal
{
    private readonly Mob _mob;

    public FloatGoal(Mob mob)
    {
        _mob = mob;
    }

    public override bool CanUse => true;

    public override bool RequiresUpdateEveryTick => true;

    public override void Tick()
    {
        // TODO: Float on fluid
    }
}