using MiaCrate.Net.Packets;
using MiaCrate.Server.Levels;

namespace MiaCrate.Server.Net;

public interface IServerPlayerConnection
{
    public ServerPlayer Player { get; }

    public void Send(IPacket packet);
}