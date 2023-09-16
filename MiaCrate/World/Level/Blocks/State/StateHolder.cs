using MiaCrate.Collections;
using MiaCrate.Data.Codecs;

namespace MiaCrate.World.Blocks;

public interface IStateHolder
{
    public const string NameTag = "Name";
    public const string PropertiesTag = "Properties";
}

public interface IStateHolderState<T> : IStateHolder
{
    public Dictionary<IProperty, IComparable> Values { get; }

    public T SetValue<TProp>(IProperty<TProp> property, TProp value) where TProp : IComparable, IComparable<TProp>;
    public TProp GetValue<TProp>(IProperty<TProp> property) where TProp : IComparable, IComparable<TProp>;
    public void PopulateNeighbors(Dictionary<Dictionary<IProperty, IComparable>, T> map);
}

public interface IStateHolderOwner<T> : IStateHolder
{
    T Owner { get; }
}

public interface IStateHolder<TOwner, TState> : IStateHolderOwner<TOwner>, IStateHolderState<TState>
{
    
}

public class StateHolder<TOwner, TState> : IStateHolder<TOwner, TState> where TState : class
{
    private readonly IMapCodec<TState> _propertiesCodec;
    private ITable<IProperty, IComparable, TState> _neighbors = new Table<IProperty, IComparable, TState>();
    
    public Dictionary<IProperty, IComparable> Values { get; }
    protected TOwner Owner { get; }

    TOwner IStateHolderOwner<TOwner>.Owner => Owner;
    
    public StateHolder(TOwner owner, Dictionary<IProperty, IComparable> values, IMapCodec<TState> propertiesCodec)
    {
        _propertiesCodec = propertiesCodec;
        Owner = owner;
        Values = values;
    }

    public TProp GetValue<TProp>(IProperty<TProp> property) where TProp : IComparable, IComparable<TProp> =>
        (TProp) Values.GetValueOrDefault(property) ??
            throw new ArgumentException($"Cannot get property {property} as it does not exist in {Owner}");

    public TState SetValue<TProp>(IProperty<TProp> property, TProp value) where TProp : IComparable, IComparable<TProp>
    {
        var oldValue = Values.GetValueOrDefault(property) ?? 
                       throw new ArgumentException($"Cannot set property {property} as it does not exist in {Owner}");
        if (oldValue.Equals(value) || oldValue.CompareTo(value) == 0)
            return (this as TState)!;

        return _neighbors[property, value] ??
               throw new ArgumentException(
                   $"Cannot set property {property} to {value} on {Owner}, it is not an allowed value");
    }

    public void PopulateNeighbors(Dictionary<Dictionary<IProperty, IComparable>, TState> map)
    {
        
    }
}