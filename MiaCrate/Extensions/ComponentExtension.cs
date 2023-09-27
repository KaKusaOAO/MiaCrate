using MiaCrate.Localizations;
using MiaCrate.Texts;
using Mochi.Texts;
using Mochi.Utils;
using Style = MiaCrate.Texts.Style;

namespace MiaCrate.Extensions;

public static class ComponentExtension
{
    public static IMutableComponent WithColor(this IMutableComponent component, TextColor color)
    {
        if (component.Style is IColoredStyle colored)
        {
            component.Style = colored.WithColor(color);
        }

        return component;
    }

    private static IMutableComponent EnsureStyle(this IMutableComponent component)
    {
        if (component.Style is Style) return component;
        
        TextColor? color = null;
        if (component.Style is IColoredStyle colored)
        {
            color = colored.Color;
        }

        return new MutableComponent<Style>(component.Content, Style.Empty.WithColor(color));
    }

    public static IMutableComponent WithUnderlined(this IMutableComponent component, bool? underlined)
    {
        component = component.EnsureStyle();
        var style = component.Style as Style;
        component.Style = style!.WithUnderlined(underlined);
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