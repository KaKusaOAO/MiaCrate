using MiaCrate.Core;
using MiaCrate.World.Blocks;
using MiaCrate.World.Phys;
using Mochi.Utils;

namespace MiaCrate.World;

public interface IBlockGetter : ILevelHeightAccessor
{
    public BlockEntity? GetBlockEntity(BlockPos pos);

    public BlockState GetBlockState(BlockPos pos);

    #region => Default implementations
    
    public int MaxLightLevel => 15;
    
    public int GetLightEmission(BlockPos pos) => GetBlockState(pos).LightEmission;

    public IEnumerable<BlockState> GetBlockStates(AABB aabb) => BlockPos.BetweenClosed(aabb).Select(GetBlockState);

    public IOptional<T> GetBlockEntity<T>(BlockPos pos, IBlockEntityType<T> type) where T : BlockEntity =>
        BlockGetterDefaults.GetBlockEntity(this, pos, type);

    #endregion

    protected static class BlockGetterDefaults
    {
        public static IOptional<T> GetBlockEntity<T>(IBlockGetter self, BlockPos pos, IBlockEntityType<T> type) where T : BlockEntity
        {
            var blockEntity = self.GetBlockEntity(pos);
            return blockEntity != null && blockEntity.Type == type
                ? Optional.Of((T) blockEntity)
                : Optional.Empty<T>();
        }
    }
}