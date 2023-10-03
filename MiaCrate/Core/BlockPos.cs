namespace MiaCrate.Core;

public class BlockPos : Vec3I, IEquatable<BlockPos>
{
    public static readonly BlockPos Zero = new(0, 0, 0);
    
    private const int LongSizeBytes = sizeof(long) * 8;
    
    private static int PackedXLength { get; } = 1 + Util.Log2(Util.SmallestEncompassingPowerOfTwo(30000000));
    private static int PackedZLength { get; } = PackedXLength;
    private static int PackedYLength { get; } = LongSizeBytes - PackedXLength - PackedZLength;
    private static long PackedXMask { get; } = (1L << PackedXLength) - 1L;
    private static long PackedYMask { get; } = (1L << PackedYLength) - 1L;
    private static long PackedZMask { get; } = (1L << PackedZLength) - 1L;
    private static int YOffset { get; } = 0;
    private static int ZOffset { get; } = PackedYLength;
    private static int XOffset { get; } = PackedYLength + PackedZLength;

    public BlockPos(int x, int y, int z) : base(x, y, z)
    {
    }
    
    public BlockPos(Vec3I v) : this(v.X, v.Y, v.Z) {}

    public static int GetX(long l) =>
        (int) (l << (LongSizeBytes - XOffset - PackedXLength) >> (LongSizeBytes - PackedXLength));
    
    public static int GetY(long l) =>
        (int) (l << (LongSizeBytes - YOffset - PackedYLength) >> (LongSizeBytes - PackedYLength));
    
    public static int GetZ(long l) =>
        (int) (l << (LongSizeBytes - ZOffset - PackedZLength) >> (LongSizeBytes - PackedZLength));
    
    public static long AsLong(int x, int y, int z)
    {
        var l = 0L;
        l |= (x & PackedXMask) << XOffset;
        l |= (y & PackedYMask) << YOffset;
        l |= (z & PackedZMask) << ZOffset;
        return l;
    }

    public override bool Equals(object? obj) => obj is BlockPos other && Equals(other);

    public bool Equals(BlockPos? other)
    {
        if (other == null) return false;
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        return HashCode.Combine(X, Y, Z);
    }
}