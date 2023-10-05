using MiaCrate.Core;
using MiaCrate.Server;
using MiaCrate.World.Blocks;

namespace MiaCrate.World;

public interface ILevelAccessor : ICommonLevelAccessor, ILevelTimeAccess
{
    public ILevelData LevelData { get; }
    
    public GameServer? Server { get; }

    public new long DayTime => LevelData.DayTime;
    long ILevelTimeAccess.DayTime => DayTime;
    
    public ChunkSource ChunkSource { get; }
    public IRandomSource Random { get; }
    
    public void BlockUpdated(BlockPos pos, Block block) {}
}