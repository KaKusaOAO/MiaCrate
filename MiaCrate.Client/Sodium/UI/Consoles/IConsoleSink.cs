using Mochi.Texts;

namespace MiaCrate.Client.Sodium.UI;

public interface IConsoleSink
{
    public void LogMessage(MessageLevel level, IComponent component, double duration);
}