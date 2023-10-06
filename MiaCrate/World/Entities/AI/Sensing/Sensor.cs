using MiaCrate.Server.Levels;
using MiaCrate.World.Entities.AI.Memory;

namespace MiaCrate.World.Entities.AI.Sensing;

public interface ISensor
{
    
}

public interface ISensor<T> : ISensor where T : LivingEntity
{
    
}

public abstract class Sensor<T> : ISensor<T> where T : LivingEntity
{
    public abstract HashSet<IMemoryModuleType> RequiredModules { get; }
    
    protected abstract void DoTick(ServerLevel level, T entity);
}