namespace MiaCrate.Core;

public interface IRegistryEntry
{
    
}

public interface IRegistryEntry<T> : IRegistryEntry where T : class
{
    public IReferenceHolder<T> BuiltinRegistryHolder { get; }    
}