namespace MiaCrate.Core;

public interface IBuiltinRegistryEntryWithHolder
{
    
}

public interface IBuiltinRegistryEntryWithHolder<T> : IBuiltinRegistryEntryWithHolder where T : class
{
    public IReferenceHolder<T> BuiltinRegistryHolder { get; }    
}