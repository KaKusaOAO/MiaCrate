namespace Mochi.Net.Packets.Status;

public interface IServerStatusPacketHandler : IServerPacketHandler
{
    public void HandleStatusRequest(ServerboundStatusRequestPacket packet);
}