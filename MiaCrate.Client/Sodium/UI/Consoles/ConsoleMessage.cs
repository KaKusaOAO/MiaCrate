using Mochi.Texts;

namespace MiaCrate.Client.Sodium.UI;

public record ConsoleMessage(MessageLevel Level, IComponent Text, double Duration);