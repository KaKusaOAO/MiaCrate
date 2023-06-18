namespace MiaCrate.Data.Utils;

public static class Extensions
{
    public static IApplicative<TLeft, TRight> Boxed<TLeft, TRight>(this IApplicative<TLeft, TRight> applicative)
        where TLeft : IK1 where TRight : IApplicative.IMu => applicative;
}