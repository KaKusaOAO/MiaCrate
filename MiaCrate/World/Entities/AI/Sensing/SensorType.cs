using MiaCrate.Core;

namespace MiaCrate.World.Entities.AI.Sensing;

public interface ISensorType
{
    public static ISensorType<DummySensor> Dummy { get; } = Register("dummy", () => new DummySensor());

    public ISensor Create();
    
    private static ISensorType<T> Register<T>(string name, Func<T> func) where T : ISensor
    {
        return (ISensorType<T>) 
            Registry.Register(BuiltinRegistries.SensorType, new ResourceLocation(name), new SensorType<T>(func));
    }
}

public interface ISensorType<out T> : ISensorType where T : ISensor
{
    public new T Create();
    ISensor ISensorType.Create() => Create();
}

public class SensorType<T> : ISensorType<T> where T : ISensor
{
    private readonly Func<T> _factory;

    public SensorType(Func<T> factory)
    {
        _factory = factory;
    }

    public T Create() => _factory();
}