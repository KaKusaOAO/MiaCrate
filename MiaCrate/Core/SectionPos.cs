using MiaCrate.World.Entities;

namespace MiaCrate.Core;

public class SectionPos : Vec3I
{
    public const int SectionBits = 4;
    public const int SectionSize = 1 << SectionBits;
    public const int SectionMask = SectionSize - 1;
    public const int SectionHalfSize = SectionSize / 2;
    public const int SectionMaxIndex = SectionMask;

    private const int LongSizeBytes = sizeof(long) * 8;
    private const int PackedXLength = 22; // How is this decided?
    private const int PackedZLength = PackedXLength;
    private const int PackedYLength = LongSizeBytes - PackedXLength - PackedZLength;
    private const long PackedXMask = (1 << PackedXLength) - 1;
    private const long PackedYMask = (1 << PackedYLength) - 1;
    private const long PackedZMask = (1 << PackedZLength) - 1;
    private const int YOffset = 0;
    private const int ZOffset = YOffset + PackedYLength;
    private const int XOffset = ZOffset + PackedZLength;
    private const int RelativeYShift = 0;
    private const int RelativeZShift = RelativeYShift + SectionBits;
    private const int RelativeXShift = RelativeZShift + SectionBits;

    public int MinBlockX => SectionToBlockCoord(X);
    public int MinBlockY => SectionToBlockCoord(Y);
    public int MinBlockZ => SectionToBlockCoord(Z);

    public int MaxBlockX => SectionToBlockCoord(X, SectionMask);
    public int MaxBlockY => SectionToBlockCoord(Y, SectionMask);
    public int MaxBlockZ => SectionToBlockCoord(Z, SectionMask);
    
    private SectionPos(int x, int y, int z) : base(x, y, z) {}

    public static SectionPos Of(int x, int y, int z) => new(x, y, z);
    public static SectionPos Of(BlockPos pos) => new(
        BlockToSectionCoord(pos.X),
        BlockToSectionCoord(pos.Y),
        BlockToSectionCoord(pos.Z));

    public static SectionPos Of(IEntityAccess entity) => Of(entity.BlockPosition);
    
    public static SectionPos Of(IPosition pos) => new(
        BlockToSectionCoord(pos.X),
        BlockToSectionCoord(pos.Y),
        BlockToSectionCoord(pos.Z));

    public static SectionPos Of(long l) => new(GetX(l), GetY(l), GetZ(l));

    public static int SectionRelativeX(short s) => s >>> RelativeXShift & SectionMask;
    public static int SectionRelativeY(short s) => s >>> RelativeYShift & SectionMask;
    public static int SectionRelativeZ(short s) => s >>> RelativeZShift & SectionMask;

    public static int BlockToSectionCoord(int blockCoord) => blockCoord >> SectionBits;
    public static int BlockToSectionCoord(double blockCoord) => (int) Math.Floor(blockCoord) >> SectionBits;
    public static int SectionToBlockCoord(int sectionCoord, int blockOffset = 0) => 
        (sectionCoord << SectionBits) + blockOffset;

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

    public static long BlockToSection(long l) => AsLong(
        BlockToSectionCoord(BlockPos.GetX(l)),
        BlockToSectionCoord(BlockPos.GetY(l)),
        BlockToSectionCoord(BlockPos.GetZ(l)));

}