namespace Mochi.Net.Packets.Handshake;

public interface IServerHandshakePacketHandler : IServerPacketHandler
{
    public void Handle(ServerboundHandshakePacket packet);
}