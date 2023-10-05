namespace MiaCrate.Core;

public class MutableBlockPos : BlockPos
{
    public MutableBlockPos(int x, int y, int z) : base(x, y, z)
    {
    }
    
    public MutableBlockPos() : this(0, 0, 0) {}
    
    public MutableBlockPos(double x, double y, double z)
        : this((int) Math.Floor(x), (int) Math.Floor(y), (int) Math.Floor(z)) {}

    public MutableBlockPos Set(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
        return this;
    }
}