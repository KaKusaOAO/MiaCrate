namespace MiaCrate.Client.Sodium.UI.Options;

public interface IOptionBinding
{
    
}

public interface IOptionBindingStorage<T> : IOptionBinding
{
    
}

public interface IOptionBindingValue<T> : IOptionBinding
{
    
}

public interface IOptionBinding<TStorage, TValue> : IOptionBindingStorage<TStorage>, IOptionBindingValue<TValue>
{
    public TValue GetValue(TStorage storage);
    public void SetValue(TStorage storage, TValue value);
}