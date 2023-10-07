namespace MiaCrate.Client.Sodium.UI.Options;

public class GenericBinding<TStorage, TValue> : IOptionBinding<TStorage, TValue>
{
    private readonly Action<TStorage, TValue> _setter;
    private readonly Func<TStorage, TValue> _getter;
    
    public GenericBinding(Action<TStorage, TValue> setter, Func<TStorage, TValue> getter)
    {
        _setter = setter;
        _getter = getter;
    }

    public TValue GetValue(TStorage storage) => _getter(storage);

    public void SetValue(TStorage storage, TValue value) => _setter(storage, value);
}