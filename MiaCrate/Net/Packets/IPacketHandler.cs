namespace MiaCrate.Net.Packets;

public interface IPacketHandler
{
    public PacketFlow Flow { get; }
}