using System.Text.RegularExpressions;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate.World.Blocks;

public interface IStateDefinition
{
    public ICollection<IProperty> Properties { get; }
    public IProperty? GetProperty(string name);
}

public interface IStateDefinitionOwner<T> : IStateDefinition
{
    public T Owner { get; }
}

public interface IStateDefinitionState<T> : IStateDefinition where T : IStateHolderState<T>
{
    public List<T> PossibleStates { get; }
    public T Any => PossibleStates.First();
}

public interface IStateDefinition<TOwner, TState> : IStateDefinitionOwner<TOwner>, IStateDefinitionState<TState> where TState : IStateHolder<TOwner, TState>
{
}

public abstract class StateDefinitionBase : IStateDefinition
{
    protected static readonly Regex NamePattern = new("^[a-z0-9_]+$");
    
    public abstract ICollection<IProperty> Properties { get; }
    
    public abstract IProperty? GetProperty(string name);
}

public class StateDefinition<TOwner, TState> : StateDefinitionBase, IStateDefinition<TOwner, TState> where TState : IStateHolder<TOwner, TState>
{
    private readonly SortedDictionary<string, IProperty> _propertiesByName;
    private readonly List<TState> _states;

    public TOwner Owner { get; }
    public List<TState> PossibleStates => new(_states);
    public override ICollection<IProperty> Properties => _propertiesByName.Values;

    private StateDefinition(Func<TOwner, TState> func, TOwner owner, Factory factory, Dictionary<string, IProperty> properties)
    {
        Owner = owner;
        _propertiesByName = new SortedDictionary<string, IProperty>(properties);
        
        var supplier = () => func(owner);
        var codec = MapCodec.Of(Encoder.Empty<TState>(), Decoder.Unit(supplier));
        
        foreach (var (key, value) in _propertiesByName)
        {
            codec = AppendPropertyCodec(codec, supplier, key, value);
        }

        var map = new Dictionary<Dictionary<IProperty, IComparable>, TState>();
        var list = new List<TState>();
        var stream = Enumerable.Empty<List<IPair<IProperty, IComparable>>>();

        foreach (var property in _propertiesByName.Values)
        {
            stream = stream.SelectMany(l => property.PossibleValues.Select(c =>
            {
                var result = new List<IPair<IProperty, IComparable>>(l)
                {
                    Pair.Of(property, c)
                };
                return result;
            }));
        }
        
        foreach (var pairs in stream.ToList())
        {
            var m = pairs
                .ToDictionary(p => p.First!, p => p.Second!);
            var holder = factory(owner, m, codec);
            map[m] = holder;
            list.Add(holder);
        }
        
        foreach (var state in list)
        {
            state.PopulateNeighbors(map);
        }

        _states = list;
    }

    private static IMapCodec<TState> AppendPropertyCodec(IMapCodec<TState> codec, Func<TState> func, string str,
        IProperty property)
    {
        throw new NotImplementedException();
    }

    public override IProperty? GetProperty(string name) => _propertiesByName.GetValueOrDefault(name);

    public delegate TState Factory(TOwner owner, Dictionary<IProperty, IComparable> map, IMapCodec<TState> codec);
    
    public class Builder
    {
        private readonly TOwner _owner;
        private readonly Dictionary<string, IProperty> _properties = new();

        public Builder(TOwner owner)
        {
            _owner = owner;
        }

        public Builder Add(params IProperty[] props)
        {
            foreach (var property in props)
            {
                ValidateProperty(property);
                _properties[property.Name] = property;
            }

            return this;
        }

        public IStateDefinition<TOwner, TState> Create(Func<TOwner, TState> func, Factory factory) => 
            new StateDefinition<TOwner, TState>(func, _owner, factory, _properties);

        private void ValidateProperty(IProperty property)
        {
            var name = property.Name;
            if (!NamePattern.IsMatch(name))
                throw new ArgumentException($"{_owner} has invalidly named property: {name}");

            var values = property.PossibleValues;
            if (values.Count <= 1)
                throw new ArgumentException($"{_owner} attempted use property {name} with <= 1 possible values");

            var invalidValueNames = values
                .Select(property.GetName)
                .Where(serialized => !NamePattern.IsMatch(serialized))
                .ToList();

            if (invalidValueNames.Any())
            {
                var joined = string.Join(", ", invalidValueNames);
                throw new ArgumentException(
                    $"{_owner} has property: {name} with invalidly named values: {joined}");
            }
            
            if (_properties.ContainsKey(name))
                throw new ArgumentException($"{_owner} has duplicate property: {name}");
        }
    }
}