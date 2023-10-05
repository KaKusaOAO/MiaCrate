using MiaCrate.Core;

namespace MiaCrate.World;

public class ChunkPos
{
    private const int SafetyMargin = 1056;

    public static long InvalidChunkPos { get; } = AsLong(1875066, 1875066);
    
    private const int CoordBits = 32;
    private const long CoordMask = (1L << CoordBits) - 1;
    private const int RegionBits = 5;
    public const int RegionSize = 1 << RegionBits;
    private const int RegionMask = RegionSize - 1;
    public const int RegionMaxIndex = RegionMask;
    private const int HashA = 1664525;
    private const int HashC = 1013904223;
    private const int HashZXor = -559038737;
    
    public int X { get; }
    public int Z { get; }

    public ChunkPos(int x, int z)
    {
        X = x;
        Z = z;
    }

    public ChunkPos(BlockPos pos)
    {
        X = SectionPos.BlockToSectionCoord(pos.X);
        Z = SectionPos.BlockToSectionCoord(pos.Z);
    }

    public ChunkPos(long l)
    {
        X = GetX(l);  // (int) (l & CoordMask);
        Z = GetZ(l);  // (int) ((l >>> CoordBits) & CoordMask);
    }
    
    public static long AsLong(int i, int j) => (i & CoordMask) | ((j & CoordMask) << CoordBits);

    public static long AsLong(BlockPos pos) =>
        AsLong(SectionPos.BlockToSectionCoord(pos.X), SectionPos.BlockToSectionCoord(pos.Z));

    public static int GetX(long l) => (int) (l & CoordMask);
    public static int GetZ(long l) => (int) ((l >>> CoordBits) & CoordMask);

    public static int Hash(int i, int j)
    {
        var k = HashA * i + HashC;
        var l = HashA * (j ^ HashZXor) + HashC;
        return k ^ l;
    }

    public override int GetHashCode() => Hash(X, Z);
}