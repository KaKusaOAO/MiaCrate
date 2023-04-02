using Mochi.Core;

namespace Mochi.World.Entities;

public static class EntityType
{
    public static EntityType<Pig> Pig = Register("pig", 
        EntityType<Pig>.Builder.Of((t, l) => new Pig(t, l), MobCategory.Creature));

    private static EntityType<T> Register<T>(ResourceLocation location, EntityType<T>.Builder builder) 
        where T : Entity => 
        (EntityType<T>) Registry.Register(Registry.EntityType, location, builder.Build());
}

public interface IEntityType : IRegistryEntry<IEntityType>
{
    
}

public interface IEntityType<out T> : IEntityType where T : Entity
{
    public T? Create(ILevel level);
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
        BuiltinRegistryHolder = Registry.EntityType.CreateIntrusiveHolder(this);
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