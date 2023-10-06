using Mochi.Brigadier;
using Mochi.Brigadier.Builder;

namespace MiaCrate.Extensions;

public static class BrigadierExtension
{
    public static T ExecutesCommand<TSource, T>(this IArgumentBuilder<TSource, T> self, CommandDelegate<TSource> cmd)
        where T : IArgumentBuilder<TSource, T> =>
        self.Executes(cmd);
}