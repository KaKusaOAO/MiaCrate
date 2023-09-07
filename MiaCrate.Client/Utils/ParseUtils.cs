using System.Text.RegularExpressions;

namespace MiaCrate.Client.Utils;

public static partial class ParseUtils
{
    private static readonly Regex _hertzRegex = GenerateHertzRegex();
    private static readonly Dictionary<string, long> _multipliers = new();

    private const string Hz = "Hz";
    private const string Khz = "kHz";
    private const string Mhz = "MHz";
    private const string Ghz = "GHz";
    private const string Thz = "THz";
    private const string Phz = "PHz";

    static ParseUtils()
    {
        _multipliers[Hz] = 1;
        _multipliers[Khz] = 1000L;
        _multipliers[Mhz] = 1000000L;
        _multipliers[Ghz] = 1000000000L;
        _multipliers[Thz] = 1000000000000L;
        _multipliers[Phz] = 1000000000000000L;
    }

    public static long ParseHertz(string hertz)
    {
        var matches = _hertzRegex.Matches(hertz.Trim());
        if (!matches.Any()) return -1;
        
        var match = matches.First();
        if (match.Groups.Count != 3) return -1;
        
        var mult = _multipliers.GetValueOrDefault(match.Groups[3].Value, -1);
        var value = double.Parse(match.Groups[1].Value) * mult;
        if (value > 0) return (long)value;
        
        return -1;
    }
    
    [GeneratedRegex("(\\d+(.\\d+)?) ?([kMGT]?Hz).*")]
    private static partial Regex GenerateHertzRegex();

    public static long ParseLongOrDefault(string s, long defValue)
    {
        try
        {
            return long.Parse(s);
        }
        catch
        {
            return defValue;
        }
    }
}