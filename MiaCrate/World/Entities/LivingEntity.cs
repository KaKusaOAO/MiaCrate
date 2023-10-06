using MiaCrate.Data;
using MiaCrate.Nbt;
using MiaCrate.Net.Data;
using MiaCrate.World.Damages;
using MiaCrate.World.Entities.AI;
using Mochi.Nbt;
using Attribute = MiaCrate.World.Entities.AI.Attribute;

namespace MiaCrate.World.Entities;

public abstract class LivingEntity : Entity, IAttackable
{
    public const int HandSlots = 2;
    public const int ArmorSlots = 4;
    public const int EquipmentSlotOffset = 98;
    public const int ArmorSlotOffset = EquipmentSlotOffset + HandSlots;
    public const int SwingDuration = 6;
    public const int PlayerHurtExperienceTime = 5 * SharedConstants.TicksPerSecond;
    private const int DamageSourceTimeout = 2 * SharedConstants.TicksPerSecond;
    public const double MinMovementDistance = 0.003;
    public const double DefaultBaseGravity = 0.08;
    public const int DeathDuration = SharedConstants.TicksPerSecond;
    private const int WaitTicksBeforeItemUseEffects = 7;
    private const int TicksPerElytraFreeFallEvent = SharedConstants.TicksPerSecond / 2;
    private const int FreeFallEventsPerElytraBreak = 2;
    public const int UseItemInterval = 4;
    private const float BaseJumpPower = 0.42f;
    private const double MaxLineOfSightTestRange = 128;
    protected const float DefaultEyeHeight = 1.74f;
    public const float ExtraRenderCullingSizeWithBigHat = 0.5f;
    private const int MaxHeadRotationRelativeToBody = 50;

    protected static IEntityDataAccessor<byte> DataLivingEntityFlags { get; } =
        SynchedEntityData.DefineId<Entity, byte>(EntityDataSerializers.Byte);

    private static IEntityDataAccessor<int> DataEffectColor { get; } =
        SynchedEntityData.DefineId<Entity, int>(EntityDataSerializers.Int);

    private float _absorptionAmount;

    public IBrain Brain { get; protected set; }
    public AttributeMap Attributes => throw new NotImplementedException();
    public LivingEntity? LastAttacker => throw new NotImplementedException();
    protected virtual IBrainProvider BrainProvider => throw new NotImplementedException();
    
    public bool IsUsingItem => 
        ((LivingEntityFlag) EntityData.Get(DataLivingEntityFlags)).HasFlag(LivingEntityFlag.IsUsing);
    public bool IsAutoSpinAttack => 
        ((LivingEntityFlag) EntityData.Get(DataLivingEntityFlags)).HasFlag(LivingEntityFlag.SpinAttack);
    
    public InteractionHand UsedItemHand =>
        ((LivingEntityFlag) EntityData.Get(DataLivingEntityFlags)).HasFlag(LivingEntityFlag.OffHand)
            ? InteractionHand.OffHand
            : InteractionHand.MainHand;

    public float MaxHealth => (float) GetAttributeValue(AI.Attributes.MaxHealth);
    public float MaxAbsorption => (float) GetAttributeValue(AI.Attributes.MaxAbsorption);

    public virtual bool IsSensitiveToWater => false;
    
    public virtual float AbsorptionAmount
    {
        get => _absorptionAmount;
        set => _absorptionAmount = Math.Clamp(value, 0, MaxAbsorption);
    }
    
    protected LivingEntity(IEntityType type, Level level) : base(type, level)
    {
        Util.LogFoobar();

        var ops = NbtOps.Instance;
        Brain = MakeBrain(new Dynamic<NbtTag>(ops, ops.CreateMap(new Dictionary<NbtTag, NbtTag>
        {
            [ops.CreateString("memories")] = ops.EmptyMap
        })));
    }

    protected virtual IBrain MakeBrain(IDynamic dyn) => BrainProvider.MakeBrain(dyn);

    public override void Kill() => Hurt(DamageSources.GenericKill, float.MaxValue);

    public virtual bool CanAttackType(IEntityType type) => true;

    public override bool Hurt(DamageSource source, float damage)
    {
        if (IsInvulnerableTo(source)) return false;
        if (Level.IsClientSide) return false;
        throw new NotImplementedException();
    }

    protected override void DefineSynchedData()
    {
        EntityData.Define(DataLivingEntityFlags, (byte) 0);
        EntityData.Define(DataEffectColor, 0);
    }

    public double GetAttributeValue(Attribute attribute) => Attributes.GetValue(attribute);
    
    public virtual void OnEnterCombat() {}
    public virtual void OnLeaveCombat() {}

    public virtual void AiStep()
    {
        throw new NotImplementedException();
    }
    
    protected virtual void ServerAiStep() {}

    [Flags]
    protected enum LivingEntityFlag
    {
        IsUsing = 1 << 0,
        OffHand = 1 << 1,
        SpinAttack = 1 << 2
    }
}