using System.Text.RegularExpressions;
using Mochi.Utils;
using SDL2;

namespace MiaCrate.Client.Platform;

public static partial class VideoModeHelper
{
    private static readonly Regex _pattern = GenerateVideoModeTextRegex();

    public static SDL.SDL_DisplayMode? Read(string? str)
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
                if (l != 24)
                {
                    Logger.Warn($"{l}-bit not implemented, will always use 24-bit");
                }
                
                return new SDL.SDL_DisplayMode
                {
                    w = width,
                    h = height,
                    format = SDL.SDL_PIXELFORMAT_RGB24,
                    refresh_rate = rate
                };
            }
        }
        catch
        {
            // ...
        }

        return null;
    }

    public static SDL.SDL_DisplayMode Create(int width, int height, int redBits, int greenBits, int blueBits, int refreshRate)
    {
        var l = redBits + greenBits + blueBits;
        if (l != 24)
            Logger.Warn($"{l}-bit not implemented, will always use 24-bit");
        
        return new SDL.SDL_DisplayMode
        {
            w = width,
            h = height,
            format = SDL.SDL_PIXELFORMAT_RGB24,
            refresh_rate = refreshRate
        };
    }

    public static IOptional<SDL.SDL_DisplayMode> ReadOptional(string? str) => Optional.OfNullable(Read(str));

    public static string Write(this SDL.SDL_DisplayMode mode)
    {
        var bytes = SDL.SDL_BYTESPERPIXEL(mode.format);
        var bits = bytes <= 2 ? SDL.SDL_BITSPERPIXEL(mode.format) : bytes * 8;
        return $"{mode.w}x{mode.h}@{mode.refresh_rate}:{bits}";
    }

    [GeneratedRegex("^(\\d+)x(\\d+)(?:@(\\d+)(?::(\\d+))?)?$")]
    private static partial Regex GenerateVideoModeTextRegex();
}