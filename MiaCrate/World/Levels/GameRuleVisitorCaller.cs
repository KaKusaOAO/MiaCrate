namespace MiaCrate.World;

public delegate void GameRuleVisitorCaller<T>(IGameRuleTypeVisitor visitor, GameRuleKey<T> key, IGameRuleType<T> type) where T : GameRuleValue<T>;