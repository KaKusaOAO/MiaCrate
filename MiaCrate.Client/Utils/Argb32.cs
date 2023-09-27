using System.Runtime.CompilerServices;
using SkiaSharp;

namespace MiaCrate.Client.Utils;

/// <summary>
/// The native color format used by <see cref="SKColor"/>.
/// </summary>
public readonly struct Argb32
{
    public static Argb32 White { get; } = new(255, 255, 255);
    
    // -> Values are intended to be defined in reversed order here
    
    // ReSharper disable InconsistentNaming
    public readonly byte Blue;
    public readonly byte Green;
    public readonly byte Red;
    public readonly byte Alpha;

    public Argb32(byte r, byte g, byte b, byte a = byte.MaxValue)
    {
        Alpha = a;
        Red = r;
        Green = g;
        Blue = b;
    }

    public int ARGB => Unsafe.As<Argb32, int>(ref Unsafe.AsRef(this));
    
    public static implicit operator Argb32(SKColor value) => Unsafe.As<SKColor, Argb32>(ref value);
    public static implicit operator Argb32(int value) => Unsafe.As<int, Argb32>(ref value);
    public static implicit operator int(Argb32 value) => Unsafe.As<Argb32, int>(ref value);
    public static implicit operator Argb32(uint value) => Unsafe.As<uint, Argb32>(ref value);
    public static implicit operator uint(Argb32 value) => Unsafe.As<Argb32, uint>(ref value);

    public Argb32 WithRed(byte red) => new(red, Green, Blue, Alpha);
    public Argb32 WithGreen(byte green) => new(Red, green, Blue, Alpha);
    public Argb32 WithBlue(byte blue) => new(Red, Green, blue, Alpha);
    public Argb32 WithAlpha(byte alpha) => new(Red, Green, Blue, alpha);
    
    public Argb32 WithRed(int red) => WithRed((byte) (red & 0xff));
    public Argb32 WithGreen(int green) => WithGreen((byte) (green & 0xff));
    public Argb32 WithBlue(int blue) => WithBlue((byte) (blue & 0xff));
    public Argb32 WithAlpha(int alpha) => WithAlpha((byte) (alpha & 0xff));
}