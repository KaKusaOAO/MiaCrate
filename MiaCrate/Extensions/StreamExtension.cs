using Mochi.IO;

namespace MiaCrate.Extensions;

public static class StreamExtension
{
    public static void WriteUInt16(this BufferWriter stream, ushort value)
    {
        stream.WriteByte((byte) (value >> 8));
        stream.WriteByte((byte) value);
    }
}