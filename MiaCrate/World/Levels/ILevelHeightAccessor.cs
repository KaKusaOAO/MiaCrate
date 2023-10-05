using MiaCrate.Core;

namespace MiaCrate.World;

public interface ILevelHeightAccessor
{
    public int Height { get; }
    public int MinBuildHeight { get; }
    
    #region => Default implementations
    public int MaxBuildHeight => MinBuildHeight + Height;
    public int MinSection => SectionPos.BlockToSectionCoord(MinBuildHeight);
    public int MaxSection => SectionPos.BlockToSectionCoord(MaxBuildHeight);
    public int SectionsCount => MaxSection - MinSection;
    
    public bool IsOutsideBuildHeight(BlockPos pos) => IsOutsideBuildHeight(pos.Y);
    public bool IsOutsideBuildHeight(int i) => i < MinBuildHeight || i >= MaxBuildHeight;
    public int GetSectionIndex(int i) => GetSectionIndexFromSectionY(SectionPos.BlockToSectionCoord(i));
    public int GetSectionIndexFromSectionY(int i) => i - MinSection;
    public int GetSectionYFromSectionIndex(int i) => i + MinSection;
    #endregion

    public static ILevelHeightAccessor Create(int minBuildHeight, int height) => new Instance(height, minBuildHeight);

    private class Instance : ILevelHeightAccessor
    {
        public int Height { get; }
        public int MinBuildHeight { get; }
        
        public Instance(int height, int minBuildHeight)
        {
            Height = height;
            MinBuildHeight = minBuildHeight;
        }
    }
}

public static class LevelHeightAccessorExtension
{
    public static bool IsOutsideBuildHeight(this ILevelHeightAccessor self, BlockPos pos) =>
        self.IsOutsideBuildHeight(pos);

    public static bool IsOutsideBuildHeight(this ILevelHeightAccessor self, int i) =>
        self.IsOutsideBuildHeight(i);

    public static int GetSectionIndex(this ILevelHeightAccessor self, int i) =>
        self.GetSectionIndex(i);

    public static int GetSectionIndexFromSectionY(this ILevelHeightAccessor self, int i) =>
        self.GetSectionIndexFromSectionY(i);

    public static int GetSectionYFromSectionIndex(this ILevelHeightAccessor self, int i) =>
        self.GetSectionYFromSectionIndex(i);
}