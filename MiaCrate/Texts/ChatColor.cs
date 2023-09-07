using System.Globalization;
using Mochi.Structs;

namespace MiaCrate.Texts;

public class ChatColor
{
    public const char ColorChar = '\u00a7';

    private readonly string _name;
    private readonly int _ordinal;
    private readonly string _toString;
    private readonly Color? _color;

    private static readonly Dictionary<char, ChatColor> _byChar = new();
    private static readonly Dictionary<string, ChatColor> _byName = new();

    private static int _count;
    
    // @formatter:off
    // -- Colors
    public static readonly ChatColor Black      = new('0', "black",       new Color(0));
    public static readonly ChatColor DarkBlue   = new('1', "dark_blue",   new Color(0xaa));
    public static readonly ChatColor DarkGreen  = new('2', "dark_green",  new Color(0xaa00));
    public static readonly ChatColor DarkAqua   = new('3', "dark_aqua",   new Color(0xaaaa));
    public static readonly ChatColor DarkRed    = new('4', "dark_red",    new Color(0xaa0000));
    public static readonly ChatColor DarkPurple = new('5', "dark_purple", new Color(0xaa00aa));
    public static readonly ChatColor Gold       = new('6', "gold",        new Color(0xffaa00));
    public static readonly ChatColor Gray       = new('7', "gray",        new Color(0xaaaaaa));
    public static readonly ChatColor DarkGray   = new('8', "dark_gray",   new Color(0x555555));
    public static readonly ChatColor Blue       = new('9', "blue",        new Color(0x5555ff));
    public static readonly ChatColor Green      = new('a', "green",       new Color(0x55ff55));
    public static readonly ChatColor Aqua       = new('b', "aqua",        new Color(0x55ffff));
    public static readonly ChatColor Red        = new('c', "red",         new Color(0xff5555));
    public static readonly ChatColor Purple     = new('d', "purple",      new Color(0xff55ff));
    public static readonly ChatColor Yellow     = new('e', "yellow",      new Color(0xffff55));
    public static readonly ChatColor White      = new('f', "white",       new Color(0xffffff));
    
    // -- Formats
    public static readonly ChatColor Obfuscated    = new('k', "obfuscated");
    public static readonly ChatColor Bold          = new('l', "bold");
    public static readonly ChatColor Strikethrough = new('m', "strikethrough");
    public static readonly ChatColor Underline     = new('n', "underline");
    public static readonly ChatColor Italic        = new('o', "italic");
    public static readonly ChatColor Reset         = new('r', "reset");
    // @formatter:on

    private ChatColor(char code, string name, Color? color = null)
    {
        _name = name;
        _toString = ColorChar + "" + code;
        _ordinal = _count++;
        _color = color;

        _byChar.Add(code, this);
        _byName.Add(name, this);
    }

    private ChatColor(string name, string toString, int rgb)
    {
        _name = name;
        _toString = toString;
        _ordinal = -1;
        _color = new Color(rgb);
    }

    public static ChatColor Of(Color color)
    {
        return Of("#" + $"{color.RGB:x6}");
    }

    public static ChatColor Of(string name)
    {
        if(name == null)
            throw new ArgumentNullException(nameof(name), "name cannot be null");

        if(name.StartsWith("#") && name.Length == 7)
        {
            int rgb;
            try
            {
                rgb = int.Parse(name.Substring(1), NumberStyles.HexNumber);
            } catch(FormatException)
            {
                throw new ArgumentException("Illegal hex string " + name);
            }

            var magic = ColorChar + "x";
            magic = name.Substring(1).Aggregate(magic, (current, c) => current + (ColorChar + "" + c));

            return new ChatColor(name, magic, rgb);
        }

        if(_byName.TryGetValue(name, out var defined))
        {
            return defined;
        }

        throw new ArgumentException("Could not parse TextColor " + name);
    }

    public static ChatColor? Of(char code)
        => _byChar.ContainsKey(code) ? _byChar[code] : null;

    public override string ToString() => _toString;

    public string Name => _name;

    public Color? Color => _color;

    private static ChatColor[] _predefined = {
        Black,
        DarkBlue,
        DarkGreen,
        DarkAqua,
        DarkRed,
        DarkPurple,
        Gold,
        Gray,
        DarkGray,
        Blue,
        Green,
        Aqua,
        Red,
        Purple,
        Yellow,
        White
    };

    public ChatColor ToNearestPredefinedColor()
    {
        var c = _toString[1];
        if (c != 'x')
        {
            return this;
        }

        if (!Color.HasValue)
        {
            throw new Exception("Not a color");
        }

        ChatColor? closest = null;
        var cl = Color.Value;

        var smallestDiff = 0;
        foreach (var tc in _predefined)
        {
            var rAverage = (tc.Color!.Value.R + cl.R) / 2;
            var rDiff = tc.Color.Value.R - cl.R;
            var gDiff = tc.Color.Value.G - cl.G;
            var bDiff = tc.Color.Value.B - cl.B;

            var diff = ((2 + (rAverage >> 8)) * rDiff * rDiff)
                       + (4 * gDiff * gDiff)
                       + ((2 + ((255 - rAverage) >> 8)) * bDiff * bDiff);

            if (closest == null || diff < smallestDiff)
            {
                closest = tc;
                smallestDiff = diff;
            }
        }

        if (closest == null)
        {
            throw new Exception("No predefined colors!");
        }
        
        return closest;
    }
}