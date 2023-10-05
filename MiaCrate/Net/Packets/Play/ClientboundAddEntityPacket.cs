using Mochi.IO;

namespace MiaCrate.Net.Packets.Play;

public class ClientboundAddEntityPacket : IPacket<IClientPlayPacketHandler>
{
    private const double MagicalQuantization = 8000;
    private const double Limit = 3.9;

    public void Write(BufferWriter writer)
    {
        throw new NotImplementedException();
    }

    public void Handle(IClientPlayPacketHandler handler)
    {
        throw new NotImplementedException();
    }
}