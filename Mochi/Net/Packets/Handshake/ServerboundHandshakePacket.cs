using Mochi.Extensions;

namespace Mochi.Net.Packets.Handshake;

public class ServerboundHandshakePacket : IPacket<IServerHandshakePacketHandler>
{
    public int ProtocolVersion { get; set; }
    public string ServerAddress { get; set; }
    public ushort ServerPort { get; set; }
    public PacketState NextState { get; set; }

    public ServerboundHandshakePacket(int version, string address, ushort port, PacketState nextState)
    {
        ProtocolVersion = version;
        ServerAddress = address;
        ServerPort = port;
        NextState = nextState;
    }

    public ServerboundHandshakePacket(MemoryStream stream)
    {
        ProtocolVersion = stream.ReadVarInt();
        ServerAddress = stream.ReadUtf8String();
        ServerPort = stream.ReadUInt16();
        NextState = stream.ReadEnum<PacketState>();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteVarInt(ProtocolVersion);
        stream.WriteUtf8String(ServerAddress);
        stream.WriteUInt16(ServerPort);
        stream.WriteEnum(NextState);
    }

    public void Handle(IServerHandshakePacketHandler handler) => handler.Handle(this);
}