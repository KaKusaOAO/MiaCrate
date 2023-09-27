using MiaCrate.Auth;
using MiaCrate.Core;
using MiaCrate.World;
using MiaCrate.World.Entities;
using Mochi.Texts;

namespace MiaCrate.Server.Levels;

public class ServerPlayer : Player
{
    public ServerPlayer(GameServer server, ServerLevel level, GameProfile profile, ClientInformation info) 
        : base(level, level.SharedSpawnPos, level.SharedSpawnAngle, profile)
    {
        throw new NotImplementedException();
    }

    public override void SendSystemMessage(IComponent component) => SendSystemMessage(component, false);

    public void SendSystemMessage(IComponent component, bool bl)
    {
        throw new NotImplementedException();
    }
}