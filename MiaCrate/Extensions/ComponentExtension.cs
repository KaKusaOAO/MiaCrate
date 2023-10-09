using MiaCrate.Localizations;
using MiaCrate.Texts;
using Mochi.Texts;
using Mochi.Utils;
using Style = MiaCrate.Texts.Style;

namespace MiaCrate.Extensions;

public static class ComponentExtension
{
    public static T AddWithParam<T>(this T text, params IComponent[] texts) where T : IComponent
    {
        if (text.Content is FormattedContent formatted)
        {
            foreach (var t in texts)
                formatted.AddWith(t);
        }
        else if (text.Content is TranslatableContent translatable)
        {
            foreach (var t in texts)
                translatable.AddWith(t);
        }
        
        return text;
    }
    
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
    
    public static IMutableComponent WithBold(this IMutableComponent component, bool? val)
    {
        component = component.EnsureStyle();
        var style = component.Style as Style;
        component.Style = style!.WithBold(val);
        return component;
    }
    
    public static IMutableComponent WithItalic(this IMutableComponent component, bool? val)
    {
        component = component.EnsureStyle();
        var style = component.Style as Style;
        component.Style = style!.WithItalic(val);
        return component;
    }

    public static IMutableComponent WithUnderlined(this IMutableComponent component, bool? val)
    {
        component = component.EnsureStyle();
        var style = component.Style as Style;
        component.Style = style!.WithUnderlined(val);
        return component;
    }
    
    public static IMutableComponent WithObfuscated(this IMutableComponent component, bool? val)
    {
        component = component.EnsureStyle();
        var style = component.Style as Style;
        component.Style = style!.WithObfuscated(val);
        return component;
    }
    
    public static IMutableComponent WithFont(this IMutableComponent component, ResourceLocation? font)
    {
        component = component.EnsureStyle();
        var style = component.Style as Style;
        component.Style = style!.WithFont(font);
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
    
    public static IFormattedText AsFormattedText(this IComponent component) => 
        IFormattedText.FromComponent(component);

    public static FormattedCharSequence GetVisualOrderText(this IComponent component) => 
        Language.Instance.GetVisualOrder(component.AsFormattedText());
}