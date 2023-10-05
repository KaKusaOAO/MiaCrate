using MiaCrate.Server;

namespace MiaCrate.World;

public interface ILevelAccessor : ICommonLevelAccessor, ILevelTimeAccess
{
    public ILevelData LevelData { get; }
    
    public GameServer? Server { get; }

    public new long DayTime => LevelData.DayTime;
    long ILevelTimeAccess.DayTime => DayTime;
}