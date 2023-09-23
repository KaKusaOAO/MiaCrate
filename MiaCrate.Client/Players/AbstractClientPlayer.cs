using MiaCrate.Auth;
using MiaCrate.Client.Multiplayer;
using MiaCrate.World.Entities;

namespace MiaCrate.Client.Players;

public abstract class AbstractClientPlayer : Player
{
    protected AbstractClientPlayer(ClientLevel level, GameProfile profile)
        : base(level, level.SharedSpawnPos, level.SharedSpawnAngle, profile)
    {
        
    }
}