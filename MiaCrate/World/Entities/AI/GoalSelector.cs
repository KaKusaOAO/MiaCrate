namespace MiaCrate.World.Entities.AI;

public class GoalSelector
{
    private readonly HashSet<WrappedGoal> _availableGoals = new();

    public void AddGoal(int priority, Goal goal)
    {
        _availableGoals.Add(new WrappedGoal(priority, goal));
    }
    
    public void RemoveAllGoals()
    {
        _availableGoals.Clear();
    }
    
    public void RemoveGoal(Goal goal)
    {
        foreach (var w in _availableGoals.Where(g => g.IsRunning))
        {
            w.Stop();
        }
        
        _availableGoals.RemoveWhere(g => g.Goal == goal);
    }
    
    
}