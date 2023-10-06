using MiaCrate.Core;
using MiaCrate.World.Phys;
using Mochi.IO;
using Path = MiaCrate.World.Pathfinder.Path;

namespace MiaCrate.Net.Packets.Common;

public record BrainDebugPayload(BrainDump BrainDump) : ICustomPacketPayload
{
    public static ResourceLocation Identifier { get; } = new("debug/brain");

    public ResourceLocation Id => Identifier;

    public void Write(BufferWriter writer) => BrainDump.Write(writer);
}

public record BrainDump(Uuid Uuid, int Id, string Name, string Profession, int Xp, float Health, float MaxHealth,
    Vec3 Pos, string Inventory, Path? Path, bool WantsGolem, int AngerLevel, List<string> Activities,
    List<string> Behaviors, List<string> Memories, List<string> Gossips, HashSet<BlockPos> Pois,
    HashSet<BlockPos> PotentialPois)
{
    public void Write(BufferWriter writer)
    {
        
    }
}