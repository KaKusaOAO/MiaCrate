using MiaCrate.Auth;
using MiaCrate.Net.Packets;
using MiaCrate.Net.Packets.Play;

namespace MiaCrate.Client.Players;

public class ClientPacketListener : ClientCommonPacketListenerImpl, ITickablePacketHandler, IClientPlayPacketHandler
{
    public GameProfile LocalGameProfile { get; }
    
    public void Tick()
    {
        throw new NotImplementedException();
    }
}