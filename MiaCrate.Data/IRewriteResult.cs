using System.Collections;

namespace MiaCrate.Data;

public interface IRewriteResult {}
public interface IRewriteResultLeft<T> : IRewriteResult {}
public interface IRewriteResultRight<T> : IRewriteResult {}

public interface IRewriteResult<TLeft, TRight> : IRewriteResultLeft<TLeft>, IRewriteResultRight<TRight>
{
    
}

public record RewriteResult<TLeft, TRight>(IView<TLeft, TRight> View, BitArray BitArray) : IRewriteResult<TLeft, TRight>
{
    
}

public static class RewriteResult
{
    public static IRewriteResult<T, T> Nop<T>(IType<T> type)
    {
        
    }
}