using MiaCrate.World.Blocks;

namespace MiaCrate.Client.Graphics;

public class AndCondition : ICondition
{
    public const string Token = "AND";
    private readonly IEnumerable<ICondition> _conditions;

    public AndCondition(IEnumerable<ICondition> conditions)
    {
        _conditions = conditions;
    }

    public Predicate<BlockState> GetPredicate(IStateDefinition<Block, BlockState> stateDefinition)
    {
        return b => _conditions
            .Select(c => c.GetPredicate(stateDefinition))
            .All(p => p(b));
    }
}