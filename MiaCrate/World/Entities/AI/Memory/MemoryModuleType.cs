using MiaCrate.Core;
using MiaCrate.Data;
using Mochi.Core;
using Mochi.Utils;

namespace MiaCrate.World.Entities.AI.Memory;

public interface IMemoryModuleType
{
    public static IMemoryModuleType<Unit> Dummy { get; } = Register<Unit>("dummy");

    private static IMemoryModuleType<T> Register<T>(string name, ICodec<T> codec)
    {
        return (IMemoryModuleType<T>) Registry.Register(BuiltinRegistries.MemoryModuleType, new ResourceLocation(name),
            new MemoryModuleType<T>(Optional.Of(codec)));
    }
    
    private static IMemoryModuleType<T> Register<T>(string name)
    {
        return (IMemoryModuleType<T>) Registry.Register(BuiltinRegistries.MemoryModuleType, new ResourceLocation(name),
            new MemoryModuleType<T>(Optional.Empty<ICodec<T>>()));
    }
}

public interface IMemoryModuleType<T> : IMemoryModuleType
{
    
}

public class MemoryModuleType<T> : IMemoryModuleType<T>
{
    public MemoryModuleType(IOptional<ICodec<T>> codec)
    {
        Util.LogFoobar();
    }
}