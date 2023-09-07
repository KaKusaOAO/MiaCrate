using System.Text.Json;
using Mochi.IO;

namespace MiaCrate.Net.Packets.Status;

public class ServerboundStatusResponsePacket : IPacket<IClientStatusPacketHandler>
{
    public ServerStatus Status { get; set; }

    public ServerboundStatusResponsePacket(ServerStatus status)
    {
        Status = status;
    }

    public ServerboundStatusResponsePacket(BufferReader stream)
    {
        Status = JsonSerializer.Deserialize<ServerStatus>(stream.ReadUtf8String())!;
    }
    
    public void Write(BufferWriter writer)
    {
        writer.WriteUtf8String(JsonSerializer.Serialize(Status));
    }

    public void Handle(IClientStatusPacketHandler handler) => handler.HandleStatusResponse(this);
}