using MiaCrate.Commands.Arguments;
using MiaCrate.Extensions;
using Mochi.Brigadier;
using Mochi.Brigadier.Context;

namespace MiaCrate.Commands;

public static class SayCommand
{
    public static void Register(CommandDispatcher<CommandSourceStack> dispatcher)
    {
        dispatcher.Register(CommandManager.Literal("say")
            .Requires(s => s.HasPermission(PermissionLevel.GameMasters))
            .Then(CommandManager.Argument("message", MessageArgument.Message())
                .ExecutesCommand(Execute)
            )
        );
    }

    private static int Execute(CommandContext<CommandSourceStack> context)
    {
        MessageArgument.ResolveChatMessage(context, "message", m =>
        {
            var stack = context.Source;
            throw new NotImplementedException();
        });

        return 1;
    }
}