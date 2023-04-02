using Mochi.IO;

namespace MiaCrate.Net.Packets.Status;

public class ServerboundStatusRequestPacket : IPacket<IServerStatusPacketHandler>
{
    public ServerboundStatusRequestPacket()
    {
        
    }

    public ServerboundStatusRequestPacket(BufferReader stream)
    {
        
    }
    
    public void Write(BufferWriter writer)
    {
        
    }

    public void Handle(IServerStatusPacketHandler handler) => handler.HandleStatusRequest(this);
}