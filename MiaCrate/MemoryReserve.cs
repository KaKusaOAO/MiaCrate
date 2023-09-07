namespace MiaCrate;

public static class MemoryReserve
{
    private static byte[]? _reserved;
    public static void Allocate() => _reserved = new byte[0xa00000];
    public static void Release() => _reserved = null;
}