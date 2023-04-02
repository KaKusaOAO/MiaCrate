using Mochi.Net.Packets;

namespace Mochi.Events;

public class ReceivedPacketEventArgs : EventArgs
{
    public IPacket Packet { get; set; }
}