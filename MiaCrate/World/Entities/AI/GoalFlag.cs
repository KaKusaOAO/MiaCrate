namespace MiaCrate.World.Entities.AI;

// Totally AI-generated
[Flags]
public enum GoalFlag
{
    None,
    
    // @formatter:off
    Move   = 1 << 0,
    Look   = 1 << 1,
    Jump   = 1 << 2,
    Target = 1 << 3
    // @formatter:on
}