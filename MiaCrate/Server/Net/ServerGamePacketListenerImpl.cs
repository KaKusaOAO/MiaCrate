using MiaCrate.Net.Packets;
using MiaCrate.Net.Packets.Play;
using MiaCrate.Server.Levels;

namespace MiaCrate.Server.Net;

public class ServerGamePacketListenerImpl : ServerCommonPacketListenerImpl, IServerPlayPacketHandler, IServerPlayerConnection, ITickablePacketHandler
{
    public ServerPlayer Player => throw new NotImplementedException();

    public void Send(IPacket packet)
    {
        throw new NotImplementedException();
    }

    public void Tick()
    {
        throw new NotImplementedException();
    }
}