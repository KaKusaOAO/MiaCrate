using MiaCrate.Core;

namespace MiaCrate.World;

public abstract class Level : IDisposable
{
    public bool IsClientSide { get; }
    
    public BlockPos SharedSpawnPos { get; }
    
    public float SharedSpawnAngle { get; }

    public void Dispose()
    {
    }
}