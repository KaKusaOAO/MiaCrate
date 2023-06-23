using System.Text;
using JetBrains.Annotations;

namespace MiaCrate.Data;

public static class Extensions
{
    public static IApplicative<TLeft, TRight> Boxed<TLeft, TRight>(this IApplicative<TLeft, TRight> applicative)
        where TLeft : IK1 where TRight : IApplicative.IMu => applicative;

    [ContractAnnotation("str:null => null; str:notnull => notnull")]
    public static string Repeat(this string? str, int repeat)
    {
        if (str == null) return null!;
        if (repeat <= 0) return "";

        var builder = new StringBuilder();
        for (var i = 0; i < repeat; i++)
        {
            builder.Append(str);
        }

        return builder.ToString();
    }
}