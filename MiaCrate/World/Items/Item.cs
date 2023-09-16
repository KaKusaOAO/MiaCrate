using System.Drawing;
using MiaCrate.Core;
using MiaCrate.Resources;
using MiaCrate.World.Blocks;
using Mochi.Nbt;
using Mochi.Utils;
using Color = Mochi.Structs.Color;

namespace MiaCrate.World.Items;

public partial class Item : IFeatureElement, IItemLike
{
    public const int StackSizeMaximum = 64;
    public const int EatDuration = 32;
    public const int MaxBarWidth = 13;
    
    private readonly IReferenceHolder<Item> _builtInRegistryHolder;
    
    public int MaxStackSize { get; }
    public int MaxDamage { get; }
    public bool IsFireResistant { get; }
    public Rarity Rarity { get; }
    public Item? CraftingRemainingItem { get; }
    public FoodProperties? FoodProperties { get; }
    public FeatureFlagSet RequiredFeatures { get; }

    public bool CanBeDepleted => MaxDamage > 0;
    public bool HasCraftingRemainingItem => CraftingRemainingItem != null;

    public Item(ItemProperties properties)
    {
        _builtInRegistryHolder = BuiltinRegistries.Item.CreateIntrusiveHolder(this);

        Rarity = properties.Rarity;
        CraftingRemainingItem = properties.CraftingRemainingItem;
        MaxDamage = properties.MaxDamage;
        MaxStackSize = properties.MaxStackSize;
        FoodProperties = properties.FoodProperties;
        IsFireResistant = properties.IsFireResistant;
        RequiredFeatures = properties.RequiredFeatures;

        if (SharedConstants.IsRunningInIde)
        {
            var name = GetType().Name;
            if (!name.EndsWith("Item"))
            {
                Logger.Error($"Item classes should end with Item and {name} doesn't.");
            }
        }
    }

    public static Item GetByBlock(Block block) => ByBlock.GetValueOrDefault(block, Air);

    public virtual void VerifyTagAfterLoad(NbtCompound tag) {}
    
    public virtual int GetBarWidth(ItemStack stack) => 
        (int) Math.Round(MaxBarWidth - stack.DamageValue * MaxBarWidth / (double) MaxDamage);

    public virtual int GetBarColor(ItemStack stack)
    {
        var f = Math.Max(0f, (MaxDamage - stack.DamageValue) / (float) MaxDamage);
        var rgb = Color.FromHsv(f * 360 * Mth.DegToRad, 1f, 1f).RGB;
        return (0xff << 24) | rgb;
    }

    public class ItemProperties
    {
        public int MaxStackSize { get; private set; } = StackSizeMaximum;
        public int MaxDamage { get; private set; }
        public Item? CraftingRemainingItem { get; private set; }
        public Rarity Rarity { get; private set; } = Rarity.Common;
        public FoodProperties? FoodProperties { get; private set; }
        public bool IsFireResistant { get; private set; }
        public FeatureFlagSet RequiredFeatures { get; private set; } = FeatureFlags.VanillaSet;

        public ItemProperties StacksTo(int stack)
        {
            if (MaxDamage > 0)
                throw new Exception("Unable to have damage AND stack.");

            MaxStackSize = stack;
            return this;
        }

        public ItemProperties DefaultDurability(int durability) => 
            MaxDamage == 0 ? Durability(durability) : this;

        public ItemProperties Durability(int durability)
        {
            MaxDamage = durability;
            MaxStackSize = 1;
            return this;
        }

        public ItemProperties CraftRemainder(Item item)
        {
            CraftingRemainingItem = item;
            return this;
        }

        public ItemProperties WithRarity(Rarity rarity)
        {
            Rarity = rarity;
            return this;
        }

        public ItemProperties SetFireResistant()
        {
            IsFireResistant = true;
            return this;
        }

        public ItemProperties WithRequiredFeatures(params FeatureFlag[] flags)
        {
            RequiredFeatures = FeatureFlags.Registry.Subset(flags);
            return this;
        }
    }

    Item IItemLike.AsItem() => this;
}