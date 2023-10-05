using MiaCrate.Net.Packets.Common;
using MiaCrate.Server.Levels;
using MiaCrate.World;
using MiaCrate.World.Entities;
using MiaCrate.World.Entities.AI;

namespace MiaCrate.Net.Packets.Play;

public class DebugPackets
{
    public static void SendGoalSelector(Level level, Mob mob, GoalSelector selector)
    {
        if (!SharedConstants.DebugGoalSelector) return;
    }
    
    public static void SendEntityBrain(LivingEntity entity)
    {
        if (!SharedConstants.DebugBrain) return;
    }

    private static void SendPacketToAllPlayers(ServerLevel level, ICustomPacketPayload payload)
    {
        var packet = new ClientboundCustomPayloadPacket(payload);

        foreach (var player in level.ServerPlayers)
        {
            player.Connection.Send(packet);
        }
    }
}