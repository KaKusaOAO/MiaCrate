using MiaCrate.Extensions;
using Mochi.IO;

namespace MiaCrate.Net.Packets.Handshake;

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

    public ServerboundHandshakePacket(BufferReader stream)
    {
        ProtocolVersion = stream.ReadVarInt();
        ServerAddress = stream.ReadUtf8String();
        ServerPort = stream.ReadUInt16();
        NextState = stream.ReadEnum<PacketState>();
    }
    
    public void Write(BufferWriter writer)
    {
        writer.WriteVarInt(ProtocolVersion);
        writer.WriteUtf8String(ServerAddress);
        writer.WriteUInt16(ServerPort);
        writer.WriteEnum(NextState);
    }

    public void Handle(IServerHandshakePacketHandler handler) => handler.Handle(this);
}