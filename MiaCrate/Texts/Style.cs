using System.Text.Json.Nodes;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.Texts;

public class Style : IStyle<Style>, IColoredStyle<Style>
{
    public static Style Empty { get; } = new();

    public static ICodec<Style> FormattingCodec { get; } =
        RecordCodecBuilder.Create<Style>(instance => instance
            .Group(
                ExtraCodecs.TextColor.OptionalFieldOf("color")
                    .ForGetter<Style>(s => Optional.OfNullable(s.Color)),
                Codec.Bool.OptionalFieldOf("bold")
                    .ForGetter<Style>(s => Optional.OfNullable(s.Bold)),
                Codec.Bool.OptionalFieldOf("italic")
                    .ForGetter<Style>(s => Optional.OfNullable(s.Italic)),
                Codec.Bool.OptionalFieldOf("underlined")
                    .ForGetter<Style>(s => Optional.OfNullable(s.Underlined)),
                Codec.Bool.OptionalFieldOf("strikethrough")
                    .ForGetter<Style>(s => Optional.OfNullable(s.Strikethrough)),
                Codec.Bool.OptionalFieldOf("obfuscated")
                    .ForGetter<Style>(s => Optional.OfNullable(s.Obfuscated)),
                Codec.String.OptionalFieldOf("insertion")
                    .ForGetter<Style>(s => Optional.OfNullable(s.Insertion)),
                ResourceLocation.Codec.OptionalFieldOf("font")
                    .ForGetter<Style>(s => Optional.OfNullable(s.ThisFont))
            )
            .Apply(instance, Create)
        );

    public static ResourceLocation DefaultFont { get; } = new("default");

    public TextColor? Color { get; private init; }
    private bool? Bold { get; init; }
    private bool? Italic { get; init; }
    private bool? Underlined { get; init; }
    private bool? Strikethrough { get; init; }
    private bool? Obfuscated { get; init; }
    public ClickEvent? ClickEvent { get; init; }
    public HoverEvent? HoverEvent { get; init; }
    public string? Insertion { get; private init; }
    private ResourceLocation? ThisFont { get; init; }

    public ResourceLocation Font => ThisFont ?? DefaultFont;
    
    public bool IsBold => Bold == true;
    public bool IsItalic => Italic == true;
    public bool IsUnderlined => Underlined == true;
    public bool IsStrikethrough => Strikethrough == true;
    public bool IsObfuscated => Obfuscated == true;
    
    private Style()
    {
        
    }
    
    private static Style Create(IOptional<TextColor> color, 
        IOptional<bool> bold, 
        IOptional<bool> italic, 
        IOptional<bool> underlined, 
        IOptional<bool> strikethrough, 
        IOptional<bool> obfuscated, 
        IOptional<string> insertion, 
        IOptional<ResourceLocation> font)
    {
        return new Style
        {
            Color = color.OrElse(null),
            Bold = bold.Select(e => (bool?) e).OrElse(null),
            Italic = italic.Select(e => (bool?) e).OrElse(null),
            Underlined = underlined.Select(e => (bool?) e).OrElse(null),
            Strikethrough = strikethrough.Select(e => (bool?) e).OrElse(null),
            Obfuscated = obfuscated.Select(e => (bool?) e).OrElse(null),
            Insertion = insertion.OrElse(null),
            ThisFont = font.OrElse(null)
        };
    }
    
    public void SerializeInto(JsonObject obj)
    {
        if (Color != null)
        {
            // Color name fix
            var color = Color.Name;
            if (color == "purple") color = "light_purple";
            obj["color"] = color;
        }

        if (Bold != null)
            obj["bold"] = Bold.Value;

        if (Italic != null)
            obj["italic"] = Italic.Value;

        if (Underlined != null)
            obj["underlined"] = Underlined.Value;

        if (Strikethrough != null)
            obj["strikethrough"] = Strikethrough.Value;
        
        if (Obfuscated != null)
            obj["obfuscated"] = Obfuscated.Value;

        if (Insertion != null)
            obj["insertion"] = Insertion;

        if (ClickEvent != null)
        {
            obj["clickEvent"] = new JsonObject
            {
                ["action"] = ClickEvent.Action.Name,
                ["value"] = ClickEvent.Value
            };
        }

        if (HoverEvent != null)
            obj["hoverEvent"] = HoverEvent.Serialize();

        if (ThisFont != null)
            obj["font"] = ThisFont.ToString();
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
            Underlined = Underlined ?? other.Underlined,
            Strikethrough = Strikethrough ?? other.Strikethrough,
            Obfuscated = Obfuscated ?? other.Obfuscated,
            ClickEvent = ClickEvent ?? other.ClickEvent,
            HoverEvent = HoverEvent ?? other.HoverEvent,
            Insertion = Insertion ?? other.Insertion,
            ThisFont = ThisFont ?? other.ThisFont
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
    
    public Style WithUnderlined(bool? value) => new Style
    {
        Underlined = value
    }.ApplyTo(this);
    
    public Style WithStrikethrough(bool? value) => new Style
    {
        Strikethrough = value
    }.ApplyTo(this);
    
    public Style WithObfuscated(bool? value) => new Style
    {
        Obfuscated = value
    }.ApplyTo(this);

    public Style WithFont(ResourceLocation? font) => new Style
    {
        ThisFont = font
    }.ApplyTo(this);

    public Style ApplyLegacyFormat(ChatFormatting formatting)
    {
        var color = Color;
        var bold = Bold;
        var italic = Italic;
        var strikethrough = Strikethrough;
        var underlined = Underlined;
        var obfuscated = Obfuscated;

        if (formatting == ChatFormatting.Obfuscated)
            obfuscated = true;
        
        else if (formatting == ChatFormatting.Bold)
            bold = true;
        
        else if (formatting == ChatFormatting.Strikethrough)
            strikethrough = true;
        
        else if (formatting == ChatFormatting.Underline)
            underlined = true;
        
        else if (formatting == ChatFormatting.Italic)
            italic = true;
        
        else if (formatting == ChatFormatting.Reset)
            return Empty;

        else
        {
            bold = false;
            italic = false;
            strikethrough = false;
            underlined = false;
            obfuscated = false;
            color = formatting.ToTextColor();
        }

        return new Style
        {
            Color = color,
            Bold = bold,
            Italic = italic,
            Strikethrough = strikethrough,
            Underlined = underlined,
            Obfuscated = obfuscated,
            ClickEvent = ClickEvent,
            HoverEvent = HoverEvent,
            Insertion = Insertion,
            ThisFont = ThisFont
        };
    }
}