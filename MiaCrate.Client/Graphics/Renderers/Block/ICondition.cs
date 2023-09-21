using MiaCrate.World.Blocks;

namespace MiaCrate.Client.Graphics;

public interface ICondition
{
    public static ICondition True { get; } = new AlwaysTrueCondition();
    public static ICondition False { get; } = new AlwaysFalseCondition();
    
    public Predicate<BlockState> GetPredicate(IStateDefinition<Block, BlockState> stateDefinition);

    private class AlwaysTrueCondition : ICondition
    {
        public Predicate<BlockState> GetPredicate(IStateDefinition<Block, BlockState> stateDefinition) => _ => true;
    }
    
    private class AlwaysFalseCondition : ICondition
    {
        public Predicate<BlockState> GetPredicate(IStateDefinition<Block, BlockState> stateDefinition) => _ => false;
    }
}