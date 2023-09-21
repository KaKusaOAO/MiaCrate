namespace MiaCrate.Net.Packets;

public interface IServerPacketHandler : IPacketHandler
{
    PacketFlow IPacketHandler.Flow => PacketFlow.Serverbound;
}