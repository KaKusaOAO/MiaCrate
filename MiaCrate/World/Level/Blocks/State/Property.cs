using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using Mochi.Utils;

namespace MiaCrate.World.Blocks;

public interface IProperty
{
    public string Name { get; }
    public List<IComparable> PossibleValues { get; }
    
    /// <summary>
    /// Get the serialized name for the given value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public string GetName(IComparable value);
}

public interface IProperty<T> : IProperty where T : IComparable, IComparable<T>
{
    public ICodec<T> Codec { get; }
    
    public new List<T> PossibleValues { get; }
    List<IComparable> IProperty.PossibleValues => PossibleValues.Cast<IComparable>().ToList();

    /// <summary>
    /// Get the serialized name for the given value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public string GetName(T value);
    string IProperty.GetName(IComparable value) => GetName((T) value);
    
    /// <summary>
    /// Deserialize the given string to an optional value instance of type <see cref="T"/>. 
    /// </summary>
    public IOptional<T> GetValue(string serialized);
}

public abstract class Property<T> : IProperty<T> where T : IComparable, IComparable<T>
{
    private readonly Lazy<int> _hashCodeLazy;
    
    public ICodec<T> Codec { get; }
    public string Name { get; }

    protected Property(string name)
    {
        _hashCodeLazy = new Lazy<int>(GenerateHashCode);
        Codec = Data.Codec.String.CoSelectSelectMany(
            s => GetValue(s).Select(DataResult.Success)
                .OrElseGet(() => DataResult.Error<T>(() => $"Unable to read property: {this} with value: {s}")),
            GetName);
        
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

    public virtual int GenerateHashCode() => 31 * typeof(T).GetHashCode() + Name.GetHashCode();

    public sealed override int GetHashCode() => _hashCodeLazy.Value;

    public IDataResult<TState> ParseValue<TDynamic, TState>(IDynamicOps<TDynamic> ops, TState state, TDynamic value)
        where TState : IStateHolderState<TState>
    {
        var result = Codec.Parse(ops, value);
        return result
            .Select(o => state.SetValue(this, o))
            .SetPartial(state);
    }

    public class ValueRecord
    {
        public ValueRecord(Property<T> property, T value)
        {
            if (!property.PossibleValues.Contains(value))
                throw new ArgumentException($"Value {value} does not belong to property {property}");

            Property = property;
            Value = value;
        }

        public Property<T> Property { get; }
        public T Value { get; }

        public void Deconstruct(out Property<T> property, out T value)
        {
            property = Property;
            value = Value;
        }

        public override string ToString() => $"{Property.Name}={Property.GetName(Value)}";
    }
}