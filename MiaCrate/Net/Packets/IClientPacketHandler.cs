namespace MiaCrate.Net.Packets;

public interface IClientPacketHandler : IPacketHandler
{
    PacketFlow IPacketHandler.Flow => PacketFlow.Clientbound;
}