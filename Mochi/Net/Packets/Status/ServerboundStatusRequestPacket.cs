namespace Mochi.Net.Packets.Status;

public class ServerboundStatusRequestPacket : IPacket<IServerStatusPacketHandler>
{
    public ServerboundStatusRequestPacket()
    {
        
    }

    public ServerboundStatusRequestPacket(MemoryStream stream)
    {
        
    }
    
    public void Write(MemoryStream stream)
    {
        
    }

    public void Handle(IServerStatusPacketHandler handler) => handler.HandleStatusRequest(this);
}