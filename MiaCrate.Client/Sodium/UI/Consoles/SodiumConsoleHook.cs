using MiaCrate.Client.UI;

namespace MiaCrate.Client.Sodium.UI;

public static class SodiumConsoleHook
{
    public static void Render(GuiGraphics graphics, double currentTime)
    {
        ConsoleRenderer.Instance.Update(Console.ExposedInstance, currentTime);
        ConsoleRenderer.Instance.Render(graphics);
    }
}