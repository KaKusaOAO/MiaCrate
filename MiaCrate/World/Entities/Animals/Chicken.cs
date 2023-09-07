using MiaCrate.World.Entities.AI.Goals;

namespace MiaCrate.World.Entities.Animals;

public class Chicken : Animal
{
    public Chicken(IEntityType type, Level level) : base(type, level)
    {
        
    }

    protected override void RegisterGoals()
    {
        base.RegisterGoals();
        GoalSelector.AddGoal(0, new FloatGoal(this));
    }
}