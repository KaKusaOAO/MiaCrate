using MiaCrate.Data;
using MiaCrate.World.Entities.AI.Memory;
using MiaCrate.World.Entities.AI.Sensing;

namespace MiaCrate.World.Entities.AI;

public interface IBrainProvider
{
    public IBrain MakeBrain(IDynamic dyn);
}

public interface IBrainProvider<T> : IBrainProvider where T : LivingEntity
{
    public new IBrain<T> MakeBrain(IDynamic dyn);
    IBrain IBrainProvider.MakeBrain(IDynamic dyn) => MakeBrain(dyn);
}

public class BrainProvider<T> : IBrainProvider<T> where T : LivingEntity
{
    private readonly ICollection<IMemoryModuleType> _memoryTypes;
    private readonly ICollection<ISensorType> _sensorTypes;
    private readonly ICodec<IBrain<T>> _codec;

    public BrainProvider(ICollection<IMemoryModuleType> memoryTypes, ICollection<ISensorType> sensorTypes)
    {
        _memoryTypes = memoryTypes;
        _sensorTypes = sensorTypes;
        _codec = IBrain.CreateCodec<T>(memoryTypes, sensorTypes);
    }

    public IBrain<T> MakeBrain(IDynamic dyn)
    {
        throw new NotImplementedException();
    }
}