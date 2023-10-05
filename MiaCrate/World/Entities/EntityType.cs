using System.Diagnostics.CodeAnalysis;
using MiaCrate.Core;
using MiaCrate.Resources;
using MiaCrate.Tags;
using MiaCrate.World.Blocks;
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
    public bool IsFireImmune { get; }

    public Entity? Create(Level level);
    public bool Is(ITagKey<IEntityType> tag) => BuiltinRegistryHolder.Is(tag);
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
    private readonly HashSet<Block> _immuneTo;
    
    public bool IsSerialized { get; }
    public bool CanBeSummoned { get; }
    public bool IsFireImmune { get; }
    public bool CanSpawnFarFromPlayer { get; }
    public int ClientTrackingRange { get; }
    public int UpdateInterval { get; }
    public FeatureFlagSet RequiredFeatures { get; }
    public EntityDimensions Dimensions { get; }
    public float Width => Dimensions.Width;
    public float Height => Dimensions.Height;

    public IReferenceHolder<IEntityType> BuiltinRegistryHolder { get; }

    public EntityType(EntityFactoryDelegate<T> factory, MobCategory category, bool serialize, bool summon,
        bool fireImmune, bool canSpawnFarFromPlayer, HashSet<Block> immuneTo, EntityDimensions dimensions, 
        int clientTrackingRange, int updateInterval,
        FeatureFlagSet requiredFeatures)
    {
        BuiltinRegistryHolder = BuiltinRegistries.EntityType.CreateIntrusiveHolder(this);
        _factory = factory;
        _category = category;
        _immuneTo = immuneTo;
        Dimensions = dimensions;
        IsSerialized = serialize;
        CanBeSummoned = summon;
        IsFireImmune = fireImmune;
        CanSpawnFarFromPlayer = canSpawnFarFromPlayer;
        ClientTrackingRange = clientTrackingRange;
        UpdateInterval = updateInterval;
        RequiredFeatures = requiredFeatures;
    }
    
    public T? Create(Level level) => _factory(this, level);
    
    public class Builder
    {
        private readonly EntityFactoryDelegate<T> _factory;
        private readonly MobCategory _category;
        
        public HashSet<Block> ImmuneTo { get; set; } = new();
        public bool IsSerialized { get; set; } = true;
        public bool CanBeSummoned { get; set; } = true;
        public bool IsFireImmune { get; set; }
        public bool CanSpawnFarFromPlayer { get; set; }
        public int ClientTrackingRange { get; set; } = 3;
        public int UpdateInterval { get; set; } = 3;
        public EntityDimensions Dimensions { get; set; } = EntityDimensions.CreateScalable(0.6f, 1.8f);
        public FeatureFlagSet RequiredFeatures { get; set; } = FeatureFlags.VanillaSet;

        private Builder(EntityFactoryDelegate<T> factory, MobCategory category)
        {
            _factory = factory;
            _category = category;
            CanSpawnFarFromPlayer = category == MobCategory.Creature || category == MobCategory.Misc;
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
            return new EntityType<T>(_factory, _category, IsSerialized, CanBeSummoned, IsFireImmune,
                CanSpawnFarFromPlayer, ImmuneTo, Dimensions, ClientTrackingRange, UpdateInterval, RequiredFeatures);
        }
    }
}