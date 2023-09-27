namespace MiaCrate.Commands;

public interface ICommandSigningContext
{
    public static ICommandSigningContext Anonymous { get; } = new AnonymousContext();
    
    private class AnonymousContext : ICommandSigningContext
    {
        
    }
}