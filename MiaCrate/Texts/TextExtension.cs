using Mochi.Texts;

namespace MiaCrate.Texts;

public static class TextExtension
{
    public static T SetBold<T>(this T text, bool value) where T : IMutableComponent
    {
        if (text.Style is Style style)
            text.Style = style.WithBold(value);
        return text;
    }
    
    public static T SetItalic<T>(this T text, bool value) where T : IMutableComponent
    {
        if (text.Style is Style style)
            text.Style = style.WithItalic(value);
        return text;
    }
    
    public static T SetUnderline<T>(this T text, bool value) where T : IMutableComponent
    {
        if (text.Style is Style style)
            text.Style = style.WithUnderline(value);
        return text;
    }
    
    public static T SetStrikethrough<T>(this T text, bool value) where T : IMutableComponent
    {
        if (text.Style is Style style)
            text.Style = style.WithStrikethrough(value);
        return text;
    }
    
    public static T SetObfuscated<T>(this T text, bool value) where T : IMutableComponent
    {
        if (text.Style is Style style)
            text.Style = style.WithObfuscated(value);
        return text;
    }
}