using MiaCrate.Localizations;
using Mochi.Texts;
using Mochi.Utils;

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
    
    public static T AppendWith<T>(this T text, params IComponent[] texts) where T : IComponent
    {
        var content = text.Content;
        if (content is not TranslatableContent t) return text;
        
        foreach (var w in texts)
        {
            t.AddWith(w);
        }
        return text;
    }

    public static FormattedCharSequence GetVisualOrderText(this IComponent component) => 
        Language.Instance.GetVisualOrder(IFormattedText.FromComponent(component));
}