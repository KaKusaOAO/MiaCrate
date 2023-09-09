using Mochi.Texts;

namespace MiaCrate.Extensions;

public static class ComponentExtension
{
    public static IMutableComponent WithStyle(this IMutableComponent component, TextColor color)
    {
        if (component.Style is IColoredStyle colored)
        {
            component.Style = colored.WithColor(color);
        }

        return component;
    }
}