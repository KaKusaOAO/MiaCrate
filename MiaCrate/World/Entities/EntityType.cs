using MiaCrate.Core;

namespace MiaCrate.World.Entities;

public static partial class EntityType
{
    private static EntityType<T> Register<T>(ResourceLocation location, EntityType<T>.Builder builder) 
        where T : Entity => 
        (EntityType<T>) Registry.Register(BuiltinRegistries.EntityType, location, builder.Build());
}

public interface IEntityType : IRegistryEntry<IEntityType>
{
    public IReferenceHolder<IEntityType> BuiltinRegistryHolder { get; }
    public Entity? Create(ILevel level);
}

public interface IEntityType<out T> : IEntityType where T : Entity
{
    public new T? Create(ILevel level);
    Entity? IEntityType.Create(ILevel level) => Create(level);
}

public delegate T? EntityFactoryDelegate<T>(IEntityType<T> type, ILevel level) where T : Entity;

public class EntityType<T> : IEntityType<T> where T : Entity
{
    private readonly EntityFactoryDelegate<T> _factory;
    private readonly MobCategory _category;

    public T? Create(ILevel level) => _factory(this, level);

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

        public static Builder Of(EntityFactoryDelegate<T> factory, MobCategory category) => new(factory, category);
        public static Builder CreateNothing(MobCategory category) => new((_, _) => null, category);

        public EntityType<T> Build()
        {
            return new EntityType<T>(_factory, _category);
        }
    }
}