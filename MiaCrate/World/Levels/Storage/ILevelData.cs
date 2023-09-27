namespace MiaCrate.World;

public interface ILevelData
{
    // TODO: Refactor?
    public int XSpawn { get; }
    public int YSpawn { get; }
    public int ZSpawn { get; }
    public float SpawnAngle { get; }
    
    public long GameTime { get; }
    public long DayTime { get; }
    public bool IsThundering { get; }
    public bool IsRaining { get; set; }
    public bool IsHardcore { get; }
    public Difficulty Difficulty { get; }
    public bool IsDifficultyLocked { get; }
}