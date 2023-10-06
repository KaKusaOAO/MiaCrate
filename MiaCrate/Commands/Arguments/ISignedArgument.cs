using Mochi.Brigadier.Arguments;

namespace MiaCrate.Commands.Arguments;

public interface ISignedArgument
{
    
}

public interface ISignedArgument<T> : ISignedArgument, IArgumentType<T>
{
    
}