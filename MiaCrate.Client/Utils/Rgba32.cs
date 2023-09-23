using System.Runtime.CompilerServices;

namespace MiaCrate.Client.Utils;

public readonly struct Rgba32
{
    // ReSharper disable InconsistentNaming
    public readonly byte Red;
    public readonly byte Green;
    public readonly byte Blue;
    public readonly byte Alpha;

    public Rgba32(byte r, byte g, byte b, byte a = byte.MaxValue)
    {
        Red = r;
        Green = g;
        Blue = b;
        Alpha = a;
    }

    public int RGBA => Unsafe.As<Rgba32, int>(ref Unsafe.AsRef(this));

    public static implicit operator Rgba32(Argb32 value) => new(value.Red, value.Green, value.Blue, value.Alpha);
    public static implicit operator Rgba32(int value) => Unsafe.As<int, Rgba32>(ref value);
    public static implicit operator Rgba32(uint value) => Unsafe.As<uint, Rgba32>(ref value);

    public Rgba32 WithRed(byte red) => new(red, Green, Blue, Alpha);
    public Rgba32 WithGreen(byte green) => new(Red, green, Blue, Alpha);
    public Rgba32 WithBlue(byte blue) => new(Red, Green, blue, Alpha);
    public Rgba32 WithAlpha(byte alpha) => new(Red, Green, Blue, alpha);

    public Rgba32 WithRed(int red) => WithRed((byte) (red & 0xff));
    public Rgba32 WithGreen(int green) => WithGreen((byte) (green & 0xff));
    public Rgba32 WithBlue(int blue) => WithBlue((byte) (blue & 0xff));
    public Rgba32 WithAlpha(int alpha) => WithAlpha((byte) (alpha & 0xff));
}