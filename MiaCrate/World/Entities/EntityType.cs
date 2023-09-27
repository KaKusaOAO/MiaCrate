using System.Diagnostics.CodeAnalysis;
using MiaCrate.Core;
using Mochi.Utils;

namespace MiaCrate.World.Entities;

public static partial class EntityType
{
    private static EntityType<T> Register
        <[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
        (string name, EntityType<T>.Builder builder) where T : Entity
    {
        var type = builder.Build();
        var registered = Registry.Register(BuiltinRegistries.EntityType, name, type);
        if (type == registered) return type;
        
        Logger.Warn($"Registered EntityType not equal to the passed type? {type} != {registered}");
        return (EntityType<T>) registered;
    }
}

public interface IEntityType : IBuiltinRegistryEntryWithHolder<IEntityType>
{
    public Entity? Create(Level level);
}

public interface IEntityType<out T> : IEntityType where T : Entity
{
    public new T? Create(Level level);
    Entity? IEntityType.Create(Level level) => Create(level);
}

public delegate T? EntityFactoryDelegate<T>(IEntityType<T> type, Level level) where T : Entity;

public class EntityType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T> 
    : IEntityType<T> where T : Entity
{
    private readonly EntityFactoryDelegate<T> _factory;
    private readonly MobCategory _category;

    public T? Create(Level level) => _factory(this, level);

    public IReferenceHolder<IEntityType> BuiltinRegistryHolder { get; }

    public EntityType(EntityFactoryDelegate<T> factory, MobCategory category)
    {
        BuiltinRegistryHolder = BuiltinRegistries.EntityType.CreateIntrusiveHolder(this);
        _factory = factory;
        _category = category;
    }
    
    public class Builder
    {
        private readonly EntityFactoryDelegate<T> _factory;
        private readonly MobCategory _category;

        private Builder(EntityFactoryDelegate<T> factory, MobCategory category)
        {
            _factory = factory;
            _category = category;
        }

        public static Builder Of(MobCategory category)
        {
            var ctor = typeof(T).GetConstructor(new[]
            {
                typeof(IEntityType), typeof(Level)
            });

            if (ctor == null)
                throw new Exception($"Type {typeof(T)} doesn't contain a standard constructor of an entity type");
            
            var factory = new EntityFactoryDelegate<T>((t, l) => (T) ctor.Invoke(new object[] {t, l}));
            return new Builder(factory, category);
        }
        
        public static Builder Of(EntityFactoryDelegate<T> factory, MobCategory category) => new(factory, category);
        public static Builder CreateNothing(MobCategory category) => new((_, _) => null, category);

        public EntityType<T> Build()
        {
            return new EntityType<T>(_factory, _category);
        }
    }
}