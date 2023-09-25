using System.Text.RegularExpressions;
using JetBrains.Annotations;
using MiaCrate.Data;
using Mochi.Structs;
using Mochi.Texts;

namespace MiaCrate;

public sealed partial class ChatFormatting : IEnumLike<ChatFormatting>, IStringRepresentable
{
    private static readonly Dictionary<int, ChatFormatting> _values = new();

    // @formatter:off
    public static ChatFormatting Black       { get; } = new("BLACK",        '0', 0x0, 0x000000);
    public static ChatFormatting DarkBlue    { get; } = new("DARK_BLUE",    '1', 0x1, 0x0000aa);
    public static ChatFormatting DarkGreen   { get; } = new("DARK_GREEN",   '2', 0x2, 0x00aa00);
    public static ChatFormatting DarkAqua    { get; } = new("DARK_AQUA",    '3', 0x3, 0x00aaaa);
    public static ChatFormatting DarkRed     { get; } = new("DARK_RED",     '4', 0x4, 0xaa0000);
    public static ChatFormatting DarkPurple  { get; } = new("DARK_PURPLE",  '5', 0x5, 0xaa00aa);
    public static ChatFormatting Gold        { get; } = new("GOLD",         '6', 0x6, 0xffaa00);
    public static ChatFormatting Gray        { get; } = new("GRAY",         '7', 0x7, 0xaaaaaa);
    public static ChatFormatting DarkGray    { get; } = new("DARK_GRAY",    '8', 0x8, 0x555555);
    public static ChatFormatting Blue        { get; } = new("BLUE",         '9', 0x9, 0x5555ff);
    public static ChatFormatting Green       { get; } = new("GREEN",        'a', 0xa, 0x55ff55);
    public static ChatFormatting Aqua        { get; } = new("AQUA",         'b', 0xb, 0x55ffff);
    public static ChatFormatting Red         { get; } = new("RED",          'c', 0xc, 0xff5555);
    public static ChatFormatting LightPurple { get; } = new("LIGHT_PURPLE", 'd', 0xd, 0xff55ff);
    public static ChatFormatting Yellow      { get; } = new("YELLOW",       'e', 0xe, 0xffff55);
    public static ChatFormatting White       { get; } = new("WHITE",        'f', 0xf, 0xffffff);

    public static ChatFormatting Obfuscated    { get; } = new("OBFUSCATED",    'k', true); 
    public static ChatFormatting Bold          { get; } = new("BOLD",          'l', true); 
    public static ChatFormatting Strikethrough { get; } = new("STRIKETHROUGH", 'm', true); 
    public static ChatFormatting Underline     { get; } = new("UNDERLINE",     'n', true); 
    public static ChatFormatting Italic        { get; } = new("ITALIC",        'o', true); 
    // @formatter:on
    
    public static ChatFormatting Reset { get; } = new("RESET", 'r', -1, null); 
    
    public const char PrefixCode = '\u00a7';
    private const string PrefixCodeString = "\u00a7";
    
    private static readonly Regex _stripFormattingRegex = CreateStripFormattingRegex();
    private static readonly Regex _cleanNameRegex = CreateCleanNameRegex();

    public static ChatFormatting[] Values => _values.Values.ToArray();

    public static ICodec<ChatFormatting> Codec { get; } = IStringRepresentable.FromEnum(() => Values);

    private static readonly Dictionary<string, ChatFormatting> _formattingByName = Values.ToDictionary(
        e => CleanName(e._name),
        e => e);

    private static readonly Dictionary<ChatFormatting, TextColor> _toColor = Values
        .Where(f => f.IsColor)
        .ToDictionary(
            e => e,
            e => TextColor.Of(new Color(e._color!.Value.RGB)).ToNearestPredefinedColor()
        );

    private readonly string _name;

    public string Name => _name.ToLowerInvariant();
    public char Code { get; }
    public bool IsFormat { get; }
    public bool IsColor => !IsFormat && this != Reset;
    public int Id { get; }
    private readonly Rgb32? _color;
    private readonly string _toString;

    string IStringRepresentable.SerializedName => Name;

    public int Ordinal { get; }

    private ChatFormatting(string name, char code, int id, Rgb32? color)
        : this(name, code, false, id, color) { }

    private ChatFormatting(string name, char code, bool isFormat, int id = -1, Rgb32? color = null)
    {
        _name = name;
        Code = code;
        IsFormat = isFormat;
        Id = id;
        _color = color;
        _toString = $"{PrefixCode}{code}";

        Ordinal = _values.Count;
        _values[Ordinal] = this;
    }

    private static string CleanName(string name) =>
        _cleanNameRegex.Replace(name.ToLowerInvariant(), "");
    
    [ContractAnnotation("null => null; notnull => notnull")]
    public static string? StripFormatting(string? str) => 
        str == null ? null : _stripFormattingRegex.Replace(str, "");

    [ContractAnnotation("null => null; notnull => canbenull")]
    public static ChatFormatting? GetByName(string? name) =>
        name == null ? null : _formattingByName.GetValueOrDefault(name);

    public static ChatFormatting? GetById(int i) => 
        i < 0 ? Reset : Values.FirstOrDefault(f => f.Id == i);

    public static ChatFormatting? GetByCode(char c)
    {
        var d = char.ToLowerInvariant(c);
        return Values.FirstOrDefault(f => f.Code == d);
    }

    public TextColor ToTextColor()
    {
        if (!IsColor)
            throw new InvalidOperationException($"Not a color: '{Name}'");

        return _toColor[this];
    }

    [GeneratedRegex($"{PrefixCodeString}[0-9a-fk-or]", RegexOptions.IgnoreCase)]
    private static partial Regex CreateStripFormattingRegex();
    
    [GeneratedRegex("^[a-z]")]
    private static partial Regex CreateCleanNameRegex();
}