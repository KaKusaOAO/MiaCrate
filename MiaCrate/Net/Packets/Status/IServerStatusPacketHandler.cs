namespace MiaCrate.Net.Packets.Status;

public interface IServerStatusPacketHandler : IServerPacketHandler, IServerPingPacketHandler
{
    public void HandleStatusRequest(ServerboundStatusRequestPacket packet);
}