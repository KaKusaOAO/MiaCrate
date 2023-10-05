using Mochi.IO;

namespace MiaCrate.Net.Packets.Common;

public interface ICustomPacketPayload
{
    public ResourceLocation Id { get; }
    
    public void Write(BufferWriter writer);
}