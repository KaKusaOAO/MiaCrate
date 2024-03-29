﻿using MiaCrate.Core;
using MiaCrate.Data.Codecs;
using MiaCrate.Resources;

namespace MiaCrate.World.Blocks;

public abstract class BlockBehavior : IFeatureElement
{
    protected bool HasCollision { get; }
    public float ExplosionResistance { get; }
    public bool IsRandomlyTicking { get; }
    public float Friction { get; set; }
    public float SpeedFactor { get; set; }
    public float JumpFactor { get; set; }
    public FeatureFlagSet RequiredFeatures { get; }
    protected BlockProperties Properties { get; }

    protected BlockBehavior(BlockProperties properties)
    {
        HasCollision = properties.HasCollision;
        ExplosionResistance = properties.ExplosionResistance;
        IsRandomlyTicking = properties.IsRandomlyTicking;
        Friction = properties.Friction;
        SpeedFactor = properties.SpeedFactor;
        JumpFactor = properties.JumpFactor;
        RequiredFeatures = properties.RequiredFeatures;
        
        Properties = properties;
    }

    public virtual int GetDirectSignal(BlockState state, IBlockGetter level, BlockPos pos, Direction direction) => 
        Redstone.SignalNone;
    
    public class BlockProperties
    {
        public bool HasCollision { get; set; } = true;
        public Func<BlockState, int> LightEmission { get; set; } = _ => 0;
        public float ExplosionResistance { get; set; }
        public float DestroyTime { get; set; }
        public bool RequiresCorrectToolForDrops { get; set; }
        public bool IsRandomlyTicking { get; set; }
        public float Friction { get; set; }
        public float SpeedFactor { get; set; } = 1f;
        public float JumpFactor { get; set; } = 1f;
        public ResourceLocation? Drops { get; set; }
        public bool CanOcclude { get; set; } = true;
        public bool IsAir { get; set; }
        public bool IgnitedByLava { get; set; }
        public bool Liquid { get; set; }
        public bool ForceSolidOff { get; set; }
        public bool ForceSolidOn { get; set; }
        public bool SpawnTerrainParticles { get; set; } = true;
        public bool Replaceable { get; set; }
        public FeatureFlagSet RequiredFeatures { get; set; } = FeatureFlags.VanillaSet;

        public BlockProperties NoLootTable()
        {
            Drops = BuiltInLootTables.Empty;
            return this;
        }

        public BlockProperties Strength(float strength) => Strength(strength, strength);
        
        public BlockProperties Strength(float time, float resistance)
        {
            DestroyTime = time;
            ExplosionResistance = resistance;
            return this;
        }
    }

    public abstract class BlockStateBase : StateHolder<Block, BlockState>
    {
        public int LightEmission { get; }
        public bool IsAir { get; }
        
        public Block Block => Owner;
        
        protected BlockStateBase(Block owner, 
            Dictionary<IProperty, IComparable> values, IMapCodec<BlockState> propertiesCodec) 
            : base(owner, values, propertiesCodec)
        {
            var props = owner.Properties;
            LightEmission = props.LightEmission(AsState());
            IsAir = props.IsAir;
        }

        public int GetDirectSignal(IBlockGetter level, BlockPos pos, Direction direction) => 
            Block.GetDirectSignal(AsState(), level, pos, direction);

        protected abstract BlockState AsState();
    }
}