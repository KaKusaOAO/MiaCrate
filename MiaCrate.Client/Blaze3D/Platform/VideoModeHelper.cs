using System.Text.RegularExpressions;
using Mochi.Utils;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MiaCrate.Client.Platform;

public static partial class VideoModeHelper
{
    private static readonly Regex _pattern = GenerateVideoModeTextRegex();

    public static VideoMode? Read(string? str)
    {
        if (str == null) return null;

        try
        {
            var matches = _pattern.Matches(str);
            if (matches.Any())
            {
                var match = matches.First();
                var width = int.Parse(match.Groups[1].Value);
                var height = int.Parse(match.Groups[2].Value);

                var str2 = match.Groups[3].Value;
                var rate = string.IsNullOrEmpty(str2) ? 60 : int.Parse(str2);

                var str3 = match.Groups[4].Value;
                var l = string.IsNullOrEmpty(str3) ? 24 : int.Parse(str3);

                var bits = l / 3;
                return new VideoMode
                {
                    Width = width,
                    Height = height,
                    RedBits = bits,
                    GreenBits = bits,
                    BlueBits = bits,
                    RefreshRate = rate
                };
            }
        }
        catch
        {
            // ...
        }

        return null;
    }

    public static VideoMode Create(int width, int height, int redBits, int greenBits, int blueBits, int refreshRate) => 
        new()
        {
            Width = width,
            Height = height,
            RedBits = redBits,
            GreenBits = greenBits,
            BlueBits = blueBits,
            RefreshRate = refreshRate
        };

    public static bool Equals(VideoMode a, VideoMode b)
    {
        return a.Width == b.Width &&
               a.Height == b.Height &&
               a.RedBits == b.RedBits &&
               a.GreenBits == b.GreenBits &&
               a.BlueBits == b.BlueBits &&
               a.RefreshRate == b.RefreshRate;
    }
    
    public static IOptional<VideoMode> ReadOptional(string? str) => Optional.OfNullable(Read(str));

    public static string Write(this VideoMode mode) => 
        $"{mode.Width}x{mode.Height}@{mode.RefreshRate}:{mode.RedBits + mode.GreenBits + mode.BlueBits}";

    [GeneratedRegex("^(\\d+)x(\\d+)(?:@(\\d+)(?::(\\d+))?)?$")]
    private static partial Regex GenerateVideoModeTextRegex();
}