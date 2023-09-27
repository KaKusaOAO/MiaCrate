using CommandLine;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using Mochi.Utils;

namespace MiaCrate.World.Blocks;

public interface IProperty
{
    public string Name { get; }
    public List<IComparable> PossibleValues { get; }
    public ICodec<IValue> ValueCodec { get; }

    /// <summary>
    /// Get the serialized name for the given value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public string GetName(IComparable value);
    
    public IOptional<IComparable> GetValue(string serialized);

    public IDataResult<TState> ParseValue<TDynamic, TState>(IDynamicOps<TDynamic> ops, TState stateHolderState, TDynamic obj)
        where TState : IStateHolderState<TState>;

    public IValue OfValue(IComparable comparable);
    public IValue OfValue(IStateHolder stateHolder);
    
    public interface IValue
    {
        public IProperty Property { get; }
        public IComparable Value { get; }
    }
}

public interface IProperty<T> : IProperty where T : IComparable, IComparable<T>
{
    public ICodec<T> Codec { get; }
    
    public new List<T> PossibleValues { get; }
    List<IComparable> IProperty.PossibleValues => PossibleValues.Cast<IComparable>().ToList();
    
    public new ICodec<IValue> ValueCodec { get; }
    ICodec<IProperty.IValue> IProperty.ValueCodec => ValueCodec.Cast<IProperty.IValue>();

    /// <inheritdoc cref="IProperty"/>
    public string GetName(T value);
    string IProperty.GetName(IComparable value) => GetName((T) value);
    
    /// <summary>
    /// Deserialize the given string to an optional value instance of type <see cref="T"/>. 
    /// </summary>
    public new IOptional<T> GetValue(string serialized);
    IOptional<IComparable> IProperty.GetValue(string serialized) => GetValue(serialized)
        .Select(c => (IComparable) c);
    
    public IValue OfValue(T value);
    IProperty.IValue IProperty.OfValue(IComparable comparable) => OfValue((T) comparable);
    
    public new IValue OfValue(IStateHolder stateHolder);
    IProperty.IValue IProperty.OfValue(IStateHolder stateHolder) => OfValue(stateHolder);
    
    public interface IValue : IProperty.IValue
    {
        public new IProperty<T> Property { get; }
        IProperty IProperty.IValue.Property => Property;
        
        public new T Value { get; }
        IComparable IProperty.IValue.Value => Value;
    }
}

public abstract class Property<T> : IProperty<T> where T : IComparable, IComparable<T>
{
    private readonly Lazy<int> _hashCodeLazy;
    
    public ICodec<T> Codec { get; }
    public ICodec<ValueRecord> ValueCodec { get; }
    ICodec<IProperty<T>.IValue> IProperty<T>.ValueCodec => ValueCodec.Cast<IProperty<T>.IValue>();
    public string Name { get; }

    protected Property(string name)
    {
        _hashCodeLazy = new Lazy<int>(GenerateHashCode);
        Codec = Data.Codec.String.CoSelectSelectMany(
            s => GetValue(s).Select(DataResult.Success)
                .OrElseGet(() => DataResult.Error<T>(() => $"Unable to read property: {this} with value: {s}")),
            GetName);
        ValueCodec = Codec.CrossSelect(OfValue, v => v.Value);
        Name = name;
    }
    
    public abstract List<T> PossibleValues { get; }
    
    /// <summary>
    /// Get the serialized name for the given value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public abstract string GetName(T value);
    
    /// <summary>
    /// Deserialize the given string to an optional value instance of type <see cref="T"/>. 
    /// </summary>
    public abstract IOptional<T> GetValue(string serialized);

    public ValueRecord OfValue(T value) => new(this, value);
    IProperty<T>.IValue IProperty<T>.OfValue(T value) => OfValue(value);

    public ValueRecord OfValue(IStateHolder stateHolder) => new(this, stateHolder.GetValue(this));
    IProperty<T>.IValue IProperty<T>.OfValue(IStateHolder stateHolder) => OfValue(stateHolder);

    public virtual int GenerateHashCode() => 31 * typeof(T).GetHashCode() + Name.GetHashCode();

    public sealed override int GetHashCode() => _hashCodeLazy.Value;

    public override bool Equals(object? obj)
    {
        if (obj is not Property<T> property) return false;
        return property.Name == Name;
    }

    public IDataResult<TState> ParseValue<TDynamic, TState>(IDynamicOps<TDynamic> ops, TState state, TDynamic value)
        where TState : IStateHolderState<TState>
    {
        var result = Codec.Parse(ops, value);
        return result
            .Select(o => state.SetValue(this, o))
            .SetPartial(state);
    }

    public class ValueRecord : IProperty<T>.IValue
    {
        public ValueRecord(Property<T> property, T value)
        {
            if (!property.PossibleValues.Contains(value))
                throw new ArgumentException($"Value {value} does not belong to property {property}");

            Property = property;
            Value = value;
        }

        public Property<T> Property { get; }
        IProperty<T> IProperty<T>.IValue.Property => Property;

        public T Value { get; }

        public void Deconstruct(out Property<T> property, out T value)
        {
            property = Property;
            value = Value;
        }

        public override string ToString() => $"{Property.Name}={Property.GetName(Value)}";
    }
}