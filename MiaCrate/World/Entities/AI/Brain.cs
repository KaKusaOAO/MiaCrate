using MiaCrate.Data;
using MiaCrate.World.Entities.AI.Memory;
using MiaCrate.World.Entities.AI.Sensing;

namespace MiaCrate.World.Entities.AI;

public interface IBrain
{
    public static IBrainProvider<T> CreateProvider<T>(
        ICollection<IMemoryModuleType> memoryModuleTypes,
        ICollection<ISensorType> sensorTypes) where T : LivingEntity =>
        new BrainProvider<T>(memoryModuleTypes, sensorTypes);

    public static ICodec<IBrain<T>> CreateCodec<T>(
        ICollection<IMemoryModuleType> memoryModuleTypes,
        ICollection<ISensorType> sensorTypes) where T : LivingEntity
    {
        throw new NotImplementedException();
    }
}

public interface IBrain<T> : IBrain where T : LivingEntity
{
    
}

public class Brain<T> : IBrain<T> where T : LivingEntity
{
    private const int ScheduleUpdateDelay = SharedConstants.TicksPerSecond;
}