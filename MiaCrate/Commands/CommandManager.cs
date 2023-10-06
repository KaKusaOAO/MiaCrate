using Mochi.Brigadier;
using Mochi.Brigadier.Arguments;
using Mochi.Brigadier.Builder;

namespace MiaCrate.Commands;

public class CommandManager
{
    public CommandDispatcher<CommandSourceStack> Dispatcher { get; } = new();

    public CommandManager(CommandSelection selection, ICommandBuildContext context)
    {
        SayCommand.Register(Dispatcher);
        Util.LogFoobar();
    }
    
    public static LiteralArgumentBuilder<CommandSourceStack> Literal(string name) =>
        LiteralArgumentBuilder<CommandSourceStack>.Literal(name);

    public static RequiredArgumentBuilder<CommandSourceStack, T> Argument<T>(string name, IArgumentType<T> type) => 
        RequiredArgumentBuilder<CommandSourceStack>.Argument(name, type);

    public static void Validate()
    {
        var context = null as ICommandBuildContext;
        var dispatcher = new CommandManager(CommandSelection.All, context!).Dispatcher;
        
        throw new NotImplementedException();
    }
}