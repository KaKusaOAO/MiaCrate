using Mochi.World.Entities.AI.Goals;

namespace Mochi.World.Entities.Animals;

public class Chicken : Animal
{
    public Chicken(IEntityType type, ILevel level) : base(type, level)
    {
        
    }

    protected override void RegisterGoals()
    {
        base.RegisterGoals();
        GoalSelector.AddGoal(0, new FloatGoal(this));
    }
}