using MiaCrate.World;

namespace MiaCrate.Client.Multiplayer;

public class ClientLevelData : IWritableLevelData
{
    public int XSpawn
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public int YSpawn
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public int ZSpawn
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public float SpawnAngle
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public long GameTime => throw new NotImplementedException();

    public long DayTime => throw new NotImplementedException();

    public bool IsThundering => throw new NotImplementedException();

    public bool IsRaining
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public bool IsHardcore => throw new NotImplementedException();

    public Difficulty Difficulty => throw new NotImplementedException();

    public bool IsDifficultyLocked => throw new NotImplementedException();
}