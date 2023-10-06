using MiaCrate.Net;
using Mochi.Brigadier.Context;
using StringReader = Mochi.Brigadier.StringReader;

namespace MiaCrate.Commands.Arguments;

public class MessageArgument : ISignedArgument<MessageArgument.MessageData>
{
    public static MessageArgument Message() => new();
    
    public MessageData Parse(StringReader reader)
    {
        throw new NotImplementedException();
    }
    
    public class MessageData
    {
        
    }

    public static void ResolveChatMessage(CommandContext<CommandSourceStack> context, string str, Action<PlayerChatMessage> consumer)
    {
        throw new NotImplementedException();
    }
}