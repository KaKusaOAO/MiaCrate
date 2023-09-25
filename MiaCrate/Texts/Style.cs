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
                    .ForGetter<Style>(s => Optional.OfNullable(s._bold)),
                Codec.Bool.OptionalFieldOf("italic")
                    .ForGetter<Style>(s => Optional.OfNullable(s._bold)),
                Codec.Bool.OptionalFieldOf("underline")
                    .ForGetter<Style>(s => Optional.OfNullable(s._bold)),
                Codec.Bool.OptionalFieldOf("strikethrough")
                    .ForGetter<Style>(s => Optional.OfNullable(s._bold)),
                Codec.Bool.OptionalFieldOf("obfuscated")
                    .ForGetter<Style>(s => Optional.OfNullable(s._bold)),
                Codec.String.OptionalFieldOf("insertion")
                    .ForGetter<Style>(s => Optional.OfNullable(s.Insertion)),
                ResourceLocation.Codec.OptionalFieldOf("font")
                    .ForGetter<Style>(s => Optional.OfNullable(s.Font))
            )
            .Apply(instance, Create)
        );

    public static ResourceLocation DefaultFont { get; } = new("default");

    private bool? _bold;
    private bool? _italic;
    private bool? _underlined;
    private bool? _strikethrough;
    private bool? _obfuscated;
    
    public TextColor? Color { get; private init; }
    public bool IsBold => _bold == true;
    public bool IsItalic => _italic == true;
    public bool IsUnderlined => _underlined == true;
    public bool IsStrikethrough => _strikethrough == true;
    public bool IsObfuscated => _obfuscated == true;
    public string? Insertion { get; private init; }
    public ResourceLocation? Font { get; private init; }
    
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
            _bold = bold.Select(e => (bool?) e).OrElse(null),
            _italic = italic.Select(e => (bool?) e).OrElse(null),
            _underlined = underlined.Select(e => (bool?) e).OrElse(null),
            _strikethrough = strikethrough.Select(e => (bool?) e).OrElse(null),
            _obfuscated = obfuscated.Select(e => (bool?) e).OrElse(null),
            Insertion = insertion.OrElse(null),
            Font = font.OrElse(null)
        };
    }
    
    public void SerializeInto(JsonObject obj)
    {
        if (Color == null) return;
        obj["color"] = "#" + Color.Color.RGB.ToString("x6");
        Util.LogFoobar();
    }

    public Style Clear() => Empty;

    public Style ApplyTo(Style other)
    {
        if (this == Empty) return other;
        if (other == Empty) return this;

        return new Style
        {
            Color = Color ?? other.Color,
            _bold = _bold ?? other._bold,
            _italic = _italic ?? other._italic,
            _underlined = _underlined ?? other._underlined,
            _strikethrough = _strikethrough ?? other._strikethrough,
            _obfuscated = _obfuscated ?? other._obfuscated
        };
    }

    public Style WithColor(TextColor? color) => new Style
    {
        Color = color
    }.ApplyTo(this);

    public Style WithBold(bool? value) => new Style
    {
        _bold = value
    }.ApplyTo(this);
    
    public Style WithItalic(bool? value) => new Style
    {
        _italic = value
    }.ApplyTo(this);
    
    public Style WithUnderlined(bool? value) => new Style
    {
        _underlined = value
    }.ApplyTo(this);
    
    public Style WithStrikethrough(bool? value) => new Style
    {
        _strikethrough = value
    }.ApplyTo(this);
    
    public Style WithObfuscated(bool? value) => new Style
    {
        _obfuscated = value
    }.ApplyTo(this);
}