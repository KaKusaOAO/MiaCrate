namespace MiaCrate.World;

public class LevelStorageSource
{   
}

public class LevelStorageAccess : IDisposable
{
    private readonly LevelStorageSource _instance;

    public LevelStorageAccess(LevelStorageSource instance)
    {
        _instance = instance;
    }

    public void Dispose()
    {
    }
}