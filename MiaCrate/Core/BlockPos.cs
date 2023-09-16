namespace MiaCrate.Core;

public class BlockPos : Vec3I
{
    public BlockPos(int x, int y, int z) : base(x, y, z)
    {
    }
    
    public BlockPos(Vec3I v) : this(v.X, v.Y, v.Z) {}
}