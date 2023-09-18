namespace MiaCrate;

public static partial class Util
{
    private static readonly int[] _multiplyDeBruijnBitPosition =
    {
        0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5,
        10, 9
    };
    
    public static int SmallestEncompassingPowerOfTwo(int i)
    {
        var j = i - 1;
        j |= j >> 1;
        j |= j >> 2;
        j |= j >> 4;
        j |= j >> 8;
        j |= j >> 16;

        return j + 1;
    }
    
    public static bool IsPowerOfTwo(int i) => i != 0 && (i & i - 1) == 0;

    public static int CeilLog2(int i)
    {
        i = IsPowerOfTwo(i) ? i : SmallestEncompassingPowerOfTwo(i);
        return _multiplyDeBruijnBitPosition[(i * 125613361L >> 27) & 31];
    }

    public static int Log2(int i) => CeilLog2(i) - (IsPowerOfTwo(i) ? 0 : 1);
}