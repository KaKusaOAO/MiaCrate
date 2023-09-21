namespace MiaCrate.Net.Packets.Status;

public interface IClientStatusPacketHandler : IClientPacketHandler, IClientPongPacketHandler
{
    public void HandleStatusResponse(ServerboundStatusResponsePacket packet);
}