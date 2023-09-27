using Mochi.Texts;

namespace MiaCrate.Commands;

public interface ICommandSource
{
    public static ICommandSource Null { get; } = new NullCommandSource();
    
    public void SendSystemMessage(IComponent component);
    public bool AcceptsSuccess { get; }
    public bool AcceptsFailure { get; }
    public bool ShouldInformAdmins { get; }
    public bool AlwaysAccepts => false;
    
    private class NullCommandSource : ICommandSource
    {
        public void SendSystemMessage(IComponent component) { }

        public bool AcceptsSuccess => false;

        public bool AcceptsFailure => false;

        public bool ShouldInformAdmins => false;
    }
}