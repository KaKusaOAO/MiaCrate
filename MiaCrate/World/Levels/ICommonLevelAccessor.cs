using MiaCrate.Core;
using MiaCrate.World.Blocks;
using MiaCrate.World.Entities;
using Mochi.Utils;

namespace MiaCrate.World;

public interface ICommonLevelAccessor : IEntityGetter, ILevelReader, ILevelSimulatedRW
{
    public new IOptional<T> GetBlockEntity<T>(BlockPos pos, IBlockEntityType<T> type) where T : BlockEntity =>
        BlockGetterDefaults.GetBlockEntity(this, pos, type);

    IOptional<T> ILevelSimulatedReader.GetBlockEntity<T>(BlockPos pos, IBlockEntityType<T> type) =>
        GetBlockEntity(pos, type);
    IOptional<T> IBlockGetter.GetBlockEntity<T>(BlockPos pos, IBlockEntityType<T> type) =>
        GetBlockEntity(pos, type);
}

public interface ILevelSimulatedRW : ILevelSimulatedReader, ILevelWriter
{
}

public interface ILevelWriter
{
    public bool SetBlock(BlockPos pos, BlockState state, int i, int j);

    public bool SetBlock(BlockPos pos, BlockState state, int i) =>
        SetBlock(pos, state, i, 512);

    public bool RemoveBlock(BlockPos pos, bool bl);

    public bool DestroyBlock(BlockPos pos, bool bl, Entity? entity, int i);

    public bool AddFreshEntity(Entity entity) => LevelWriterDefaults.AddFreshEntity(this, entity);

    protected static class LevelWriterDefaults
    {
        public static bool AddFreshEntity(ILevelWriter self, Entity entity) => false;
    }
}