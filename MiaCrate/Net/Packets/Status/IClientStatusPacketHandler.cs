namespace MiaCrate.Net.Packets.Status;

public interface IClientStatusPacketHandler : IClientPacketHandler
{
    public void HandleStatusResponse(ServerboundStatusResponsePacket packet);
}