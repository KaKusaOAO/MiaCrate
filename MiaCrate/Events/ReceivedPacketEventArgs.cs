using MiaCrate.Net.Packets;

namespace MiaCrate.Events;

public class ReceivedPacketEventArgs : EventArgs
{
    public IPacket Packet { get; set; }
}