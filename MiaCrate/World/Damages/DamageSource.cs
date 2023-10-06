using MiaCrate.Core;
using MiaCrate.Data;
using MiaCrate.Sounds;
using MiaCrate.Tags;
using MiaCrate.World.Entities;
using MiaCrate.World.Phys;

namespace MiaCrate.World.Damages;

public class DamageSource
{
    public IHolder<DamageType> TypeHolder { get; }
    public Entity? Entity { get; }
    public Entity? DirectEntity { get; }
    public Vec3? SourcePositionRaw { get; }
    public Vec3? SourcePosition => SourcePositionRaw ?? DirectEntity?.Position;
    public bool IsCreativePlayer => Entity is Player {Abilities.InstaBuild: true};

    private DamageSource(IHolder<DamageType> holder, Entity? causingEntity, Entity? directEntity, Vec3? sourcePosition)
    {
        TypeHolder = holder;
        Entity = causingEntity;
        DirectEntity = directEntity;
        SourcePositionRaw = sourcePosition;
    }

    public DamageSource(IHolder<DamageType> holder)
        : this(holder, null, null, null) { }

    public bool Is(ITagKey<DamageType> tag) => TypeHolder.Is(tag);
}

public record DamageType(string MsgId, DamageScaling Scaling, float Exhaustion, DamageEffects Effects);

public sealed class DamageScaling : IStringRepresentable
{
    public static DamageScaling Never { get; } = new("never");
    
    public string SerializedName { get; }

    private DamageScaling(string id)
    {
        SerializedName = id;
    }
}

public sealed class DamageEffects : IStringRepresentable
{
    public static DamageEffects Hurt { get; } = new("hurt", SoundEvents.PlayerHurt);
    public static DamageEffects Thorns { get; } = new("thorns", SoundEvents.ThornsHit);
    
    public string SerializedName { get; }
    public SoundEvent Sound { get; }

    private DamageEffects(string id, SoundEvent sound)
    {
        SerializedName = id;
        Sound = sound;
    }
}

public sealed class DeathMessageType : IEnumLike<DeathMessageType>, IStringRepresentable
{
    private static readonly Dictionary<int, DeathMessageType> _values = new();

    public static DeathMessageType Default { get; } = new("default");
    public static DeathMessageType FallVariants { get; } = new("fall_variants");
    public static DeathMessageType IntentionalGameDesign { get; } = new("intentional_game_design");

    public static ICodec<DeathMessageType> Codec { get; } = IStringRepresentable.FromEnum<DeathMessageType>();

    public string SerializedName { get; }
    public int Ordinal { get; }
    public static DeathMessageType[] Values => _values.Values.ToArray();

    private DeathMessageType(string id)
    {
        SerializedName = id;
        
        Ordinal = _values.Count;
        _values[Ordinal] = this;
    }
}