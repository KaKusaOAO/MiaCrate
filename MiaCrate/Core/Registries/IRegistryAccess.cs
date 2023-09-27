using System.Collections.Immutable;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Core;

public interface IRegistryAccess
{
    public static IFrozen Empty { get; } =
        ((IRegistryAccess) new ImmutableRegistryAccess(Enumerable.Empty<IRegistry>())).Freeze();
    
    public IEnumerable<RegistryEntry> Registries { get; }
    public IOptional<IRegistry<T>> Registry<T>(IResourceKey<IRegistry<T>> key) where T : class;

    public IRegistry<T> RegistryOrThrow<T>(IResourceKey<IRegistry<T>> key) where T : class =>
        Registry(key).OrElseGet(() => 
            throw new InvalidOperationException($"Missing registry: {key}"));

    public IFrozen Freeze() => new FrozenAccess(Registries.Select(r => r.Freeze()));

    public static IFrozen FromRegistryOfRegistries(IRegistry<IRegistry> registry) =>
        new FrozenRegistryOfRegistries(registry);

    public record RegistryEntry(IResourceKey<IRegistry> Key, IRegistry Value)
    {
        public RegistryEntry Freeze() => this with {Value = Value.Freeze()};
    }
    
    public interface IFrozen : IRegistryAccess { }

    public class ImmutableRegistryAccess : IRegistryAccess
    {
        private readonly Dictionary<IResourceKey<IRegistry>, IRegistry> _registries;

        public IEnumerable<RegistryEntry> Registries => 
            _registries.Select(e => new RegistryEntry(e.Key, e.Value));

        public ImmutableRegistryAccess(IEnumerable<IRegistry> list)
        {
            _registries = list.ToDictionary(e => e.Key, e => e);
        }
        
        public ImmutableRegistryAccess(IDictionary<IResourceKey<IRegistry>, IRegistry> list)
        {
            _registries = new Dictionary<IResourceKey<IRegistry>, IRegistry>(list);
        }
        
        public ImmutableRegistryAccess(IEnumerable<RegistryEntry> list)
        {
            _registries = list.ToDictionary(e => e.Key, e => e.Value);
        }

        public IOptional<IRegistry<T>> Registry<T>(IResourceKey<IRegistry<T>> key) where T : class => 
            Optional.OfNullable(_registries[key]).Select(r => (IRegistry<T>) r);
    }

    private class FrozenAccess : ImmutableRegistryAccess, IFrozen
    {
        public FrozenAccess(IEnumerable<RegistryEntry> list) : base(list) {}
    } 

    private class FrozenRegistryOfRegistries : IFrozen
    {
        private readonly IRegistry<IRegistry> _registry;

        public FrozenRegistryOfRegistries(IRegistry<IRegistry> registry)
        {
            _registry = registry;
        }

        public IEnumerable<RegistryEntry> Registries => _registry.Select(r => new RegistryEntry(r.Key, r));

        public IOptional<IRegistry<T>> Registry<T>(IResourceKey<IRegistry<T>> key) where T : class => 
            _registry.GetOptional(key).Select(r => (IRegistry<T>) r);

        public IFrozen Freeze() => this;
    }
}