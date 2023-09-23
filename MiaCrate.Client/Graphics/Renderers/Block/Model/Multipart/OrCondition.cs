using MiaCrate.World.Blocks;

namespace MiaCrate.Client.Graphics;

public class OrCondition : ICondition
{
    public const string Token = "OR";
    private readonly IEnumerable<ICondition> _conditions;

    public OrCondition(IEnumerable<ICondition> conditions)
    {
        _conditions = conditions;
    }

    public Predicate<BlockState> GetPredicate(IStateDefinition<Block, BlockState> stateDefinition)
    {
        return b => _conditions
            .Select(c => c.GetPredicate(stateDefinition))
            .Any(p => p(b));
    }
}