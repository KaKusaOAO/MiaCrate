using MiaCrate.Extensions;
using Mochi.IO;

namespace MiaCrate.Net.Packets.Common;

public record ClientboundCustomPayloadPacket(ICustomPacketPayload Payload) : IPacket<IClientCommonPacketHandler>
{
    private const int MaxPayloadSize = 0x100000;
    
    public void Write(BufferWriter writer)
    {
        writer.WriteResourceLocation(Payload.Id);
        Payload.Write(writer);
    }

    public void Handle(IClientCommonPacketHandler handler)
    {
        throw new NotImplementedException();
    }
}