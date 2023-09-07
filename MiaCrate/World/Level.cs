namespace MiaCrate.World;

public abstract class Level : IDisposable
{
    public bool IsClientSide { get; }

    public void Dispose()
    {
    }
}