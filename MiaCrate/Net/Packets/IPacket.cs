using Mochi.IO;

namespace MiaCrate.Net.Packets;

public interface IPacket
{
    public void Write(BufferWriter writer);
    public void Handle(IPacketHandler handler);
    public ConnectionProtocol? NextProtocol => null;
}

public interface IPacket<in T> : IPacket where T: IPacketHandler
{
    public void Handle(T handler);

    void IPacket.Handle(IPacketHandler handler) => Handle((T)handler);
}