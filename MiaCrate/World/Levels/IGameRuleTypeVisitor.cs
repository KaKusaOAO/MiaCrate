namespace MiaCrate.World;

public interface IGameRuleTypeVisitor
{
    public void VisitBool(GameRuleKey<GameRuleBoolValue> key, IGameRuleType<GameRuleBoolValue> type) {}
    public void VisitInt(GameRuleKey<GameRuleIntValue> key, IGameRuleType<GameRuleIntValue> type) {}
}