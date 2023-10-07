using Mochi.Texts;

namespace MiaCrate.Client.Sodium.UI;

public class Console : IConsoleSink
{
    internal static Console ExposedInstance { get; } = new();
    
    public static IConsoleSink Instance => ExposedInstance;

    public Queue<ConsoleMessage> Messages { get; } = new();

    private Console() {}
    
    public void LogMessage(MessageLevel level, IComponent component, double duration)
    {
        Messages.Enqueue(new ConsoleMessage(level, component, duration));
    }
}