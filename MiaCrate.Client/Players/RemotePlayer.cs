using MiaCrate.Auth;
using MiaCrate.Client.Multiplayer;

namespace MiaCrate.Client.Players;

public class RemotePlayer : AbstractClientPlayer
{
    public RemotePlayer(ClientLevel level, GameProfile profile) : base(level, profile)
    {
    }
}