using MiaCrate.Localizations;
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

    public static FormattedCharSequence GetVisualOrderText(this IComponent component) => 
        Language.Instance.GetVisualOrder(IFormattedText.FromComponent(component));
}