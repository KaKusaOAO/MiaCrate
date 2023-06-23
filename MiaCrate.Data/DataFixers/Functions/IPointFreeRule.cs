using Mochi.Utils;

namespace MiaCrate.Data;

public interface IPointFreeRule
{
    public IOptional<IPointFree<T>> Rewrite<T>(IPointFree<T> expr);

    public IPointFree<T> RewriteOrNop<T>(IPointFree<T> expr) => Rewrite(expr).OrElse(expr);
}

public static class PointFreeRule
{
    public static IPointFreeRule Seq(params IPointFreeRule[] rules) => new SeqRule(rules);
    public static IPointFreeRule Nop => NopRule.Shared;

    private record SeqRule(IPointFreeRule[] Rules) : IPointFreeRule
    {
        public IOptional<IPointFree<T>> Rewrite<T>(IPointFree<T> expr)
        {
            var result = Rules.Aggregate(expr, (current, rule) => rule.RewriteOrNop(current));
            return Optional.Of(result);
        }
    }
    
    private class NopRule : IPointFreeRule
    {
        private static NopRule? _instance;
        public static NopRule Shared => _instance ??= new NopRule();
        public IOptional<IPointFree<T>> Rewrite<T>(IPointFree<T> expr) => Optional.Of(expr);
    }

    private class BangEtaRule : IPointFreeRule
    {
        private static BangEtaRule? _instance;
        public static BangEtaRule Shared => _instance ??= new BangEtaRule();

        public IOptional<IPointFree<T>> Rewrite<T>(IPointFree<T> expr)
        {
            if (expr is IBang) return Optional.Empty<IPointFree<T>>();
            if (expr.Type is IFuncType func)
            {
                if (func.Second is EmptyPart)
                {
                    return Optional.Of((IPointFree<T>) func.First.CreateBang());
                }
            }

            return Optional.Empty<IPointFree<T>>();
        }
    }
}