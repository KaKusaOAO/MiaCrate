using MiaCrate.World.Phys;
using Mochi.Texts;

namespace MiaCrate.Core;

public class BlockPos : Vec3I, IEquatable<BlockPos>, IVec3I<BlockPos>
{
    public static BlockPos Zero { get; } = new(0, 0, 0);
    
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
    
    public new static BlockPos Create(int x, int y, int z) => new(x, y, z);

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

    public static IEnumerable<BlockPos> BetweenClosed(AABB aabb) =>
        BetweenClosed((int) Math.Floor(aabb.MinX), (int) Math.Floor(aabb.MinY), (int) Math.Floor(aabb.MinZ),
            (int) Math.Floor(aabb.MaxX), (int) Math.Floor(aabb.MaxY), (int) Math.Floor(aabb.MaxZ));

    public static IEnumerable<BlockPos> BetweenClosed(BlockPos a, BlockPos b) =>
        BetweenClosed(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z),
            Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));

    public static IEnumerable<BlockPos> BetweenClosed(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
    {
        var xCount = maxX - minX + 1;
        var yCount = maxY - minY + 1;
        var zCount = maxZ - minZ + 1;
        var count = xCount * yCount * zCount;

        var cursor = new MutableBlockPos();
        var index = 0;
        
        while (true)
        {
            if (index == count) yield break;
            
            var xOffset = index % yCount;
            var yIndex = index / yCount;
            var yOffset = yIndex % zCount;
            var zOffset = yIndex / zCount;

            ++index;
            yield return cursor.Set(maxX + xOffset, maxY + yOffset, maxZ + zOffset);
        }
    }
}