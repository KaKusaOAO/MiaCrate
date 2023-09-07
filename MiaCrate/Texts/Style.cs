using System.Text.Json.Nodes;
using Mochi.Texts;

namespace MiaCrate.Texts;

public class Style : IStyle<Style>, IColoredStyle<Style>
{
    public static readonly Style Empty = new();
    
    public TextColor? Color { get; private set; }
    public bool? Bold { get; private set; }
    public bool? Italic { get; private set; }
    public bool? Underline { get; private set; }
    public bool? Strikethrough { get; private set; }
    public bool? Obfuscated { get; private set; }
    
    private Style()
    {
        
    }
    
    public void SerializeInto(JsonObject obj)
    {
        if (Color == null) return;
        obj["color"] = "#" + Color.Color.RGB.ToString("x6");
    }

    public Style Clear() => Empty;

    public Style ApplyTo(Style other)
    {
        if (this == Empty) return other;
        if (other == Empty) return this;

        return new Style
        {
            Color = Color ?? other.Color,
            Bold = Bold ?? other.Bold,
            Italic = Italic ?? other.Italic,
            Underline = Underline ?? other.Underline,
            Strikethrough = Strikethrough ?? other.Strikethrough,
            Obfuscated = Obfuscated ?? other.Obfuscated
        };
    }

    public Style WithColor(TextColor? color) => new Style
    {
        Color = color
    }.ApplyTo(this);

    public Style WithBold(bool? value) => new Style
    {
        Bold = value
    }.ApplyTo(this);
    
    public Style WithItalic(bool? value) => new Style
    {
        Italic = value
    }.ApplyTo(this);
    
    public Style WithUnderline(bool? value) => new Style
    {
        Underline = value
    }.ApplyTo(this);
    
    public Style WithStrikethrough(bool? value) => new Style
    {
        Strikethrough = value
    }.ApplyTo(this);
    
    public Style WithObfuscated(bool? value) => new Style
    {
        Obfuscated = value
    }.ApplyTo(this);
}