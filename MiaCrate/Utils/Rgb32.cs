using System.Runtime.CompilerServices;

namespace MiaCrate;

public readonly struct Rgb32
{
    // ReSharper disable InconsistentNaming
    public readonly byte Blue;
    public readonly byte Green;
    public readonly byte Red;
    
#pragma warning disable CS0414
    // This field is kept to pad the struct size to 4 bytes
    // ReSharper disable once ConvertToConstant.Local
    private readonly byte Alpha = 0;
#pragma warning restore CS0414

    public Rgb32(byte r, byte g, byte b)
    {
        Red = r;
        Green = g;
        Blue = b;
    }

    public int RGB => Unsafe.As<Rgb32, int>(ref Unsafe.AsRef(this));

    public static implicit operator Rgb32(int value)
    {
        var stripped = value & 0xffffff;
        return Unsafe.As<int, Rgb32>(ref stripped);
    }

    public static implicit operator Rgb32(uint value)
    {
        var stripped = value & 0xffffff;
        return Unsafe.As<uint, Rgb32>(ref stripped);
    }

    public Rgb32 WithRed(byte red) => new(red, Green, Blue);
    public Rgb32 WithGreen(byte green) => new(Red, green, Blue);
    public Rgb32 WithBlue(byte blue) => new(Red, Green, blue);

    public Rgb32 WithRed(int red) => WithRed((byte) (red & 0xff));
    public Rgb32 WithGreen(int green) => WithGreen((byte) (green & 0xff));
    public Rgb32 WithBlue(int blue) => WithBlue((byte) (blue & 0xff));
}