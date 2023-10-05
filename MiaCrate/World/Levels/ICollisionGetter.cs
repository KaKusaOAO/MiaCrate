using MiaCrate.World.Entities;
using MiaCrate.World.Phys.Shapes;

namespace MiaCrate.World;

public interface ICollisionGetter : IBlockGetter
{
    public WorldBorder WorldBorder { get; }

    public IBlockGetter GetChunkForCollisions(int x, int z);

    public bool IsUnobstructed(Entity? entity, VoxelShape shape) => true;
}