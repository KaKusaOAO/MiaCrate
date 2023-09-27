using System.Text;
using Mochi.IO;

namespace MiaCrate.Extensions;

public static class StreamExtension
{
    public static void WriteUInt16(this BufferWriter stream, ushort value)
    {
        stream.WriteByte((byte) (value >> 8));
        stream.WriteByte((byte) value);
    }
    
    public static void WriteResourceLocation(this BufferWriter stream, ResourceLocation location)
    {
        stream.WriteUtf8String(location.ToString());
    }

    public static string ReadUtf8String(this BufferReader stream, int maxLen)
    {
        var count = stream.ReadVarInt();
        if (count > maxLen)
            throw new InvalidOperationException($"String length {count} exceeds max length allowed: {maxLen}");
            
        var buffer = new byte[count];
        if (stream.Stream.Read(buffer, 0, count) != count)
            throw new EndOfStreamException("Could not read all bytes");
        
        return Encoding.UTF8.GetString(buffer);
    }
    
    public static ResourceLocation ReadResourceLocation(this BufferReader stream)
    {
        var str = stream.ReadUtf8String(short.MaxValue);
        return new ResourceLocation(str);
    }
}