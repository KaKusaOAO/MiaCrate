using MiaCrate.Core;
using MiaCrate.Extensions;
using MiaCrate.Resources;
using Mochi.Core;
using Mochi.Nbt;
using Mochi.Utils;

namespace MiaCrate.World.Items;

public sealed class ItemStack
{
    public const string TagEnch = "Enchantments";
    public const string TagDisplay = "display";
    public const string TagDisplayName = "Name";
    public const string TagLore = "Lore";
    public const string TagDamage = "Damage";
    public const string TagColor = "color";
    private const string TagUnbreakable = "Unbreakable";
    private const string TagRepairCost = "RepairCost";
    private const string TagCanDestroyBlockList = "CanDestroy";
    private const string TagCanPlaceOnBlockList = "CanPlaceOn";
    private const string TagHideFlags = "HideFlags";

    public static readonly ItemStack Empty = new();
    
    private readonly Item? _item;
    private NbtCompound? _compound;
    
    public int Count { get; private set; }
    public int PopTime { get; private set; }
    public Item Item => _item ?? Item.Air;
    public bool IsEmpty => this == Empty || _item == Item.Air || Count <= 0;

    public int DamageValue
    {
        get => _compound?.GetInt(TagDamage) ?? 0;
        set => GetOrCreateTag()[TagDamage] = Math.Max(0, value);
    }

    public int BarWidth => Item.GetBarWidth(this);
    public int BarColor => Item.GetBarColor(this);

    public ItemStack(Mochi.Utils.IHolder<Item> holder, int count = 1) : this(holder.Value, count) { }

    public ItemStack(IItemLike itemLike, int count, IOptional<NbtCompound> compound) : this(itemLike, count)
    {
        compound.IfPresent(SetTag);
    }
    
    public ItemStack(IItemLike itemLike, int count = 1)
    {
        _item = itemLike.AsItem();
        Count = count;
    }

    private ItemStack()
    {
        _item = null;
    }

    private ItemStack(NbtCompound tag)
    {
        _item = BuiltinRegistries.Item.Get(new ResourceLocation(tag.GetString("id")));
        Count = tag.GetByte("Count");

        var data = tag["tag"];
        if (data != null! && data.Type == NbtTag.TagType.Compound)
        {
            _compound = data.As<NbtCompound>();
            Item.VerifyTagAfterLoad(_compound);
        }

        if (Item.CanBeDepleted)
        {
            // Seems redundant but it ensures damage value is set in NBT tag
            DamageValue = DamageValue;
        }
    }

    public static ItemStack Of(NbtCompound compound)
    {
        try
        {
            return new ItemStack(compound);
        }
        catch (Exception ex)
        {
            Logger.Verbose($"Tried to load invalid item: {compound}");
            Logger.Verbose(ex);
            return Empty;
        }
    }

    public bool IsItemEnabled(FeatureFlagSet features) => IsEmpty || Item.IsEnabled(features);

    public NbtCompound GetOrCreateTag()
    {
        if (_compound == null)
            SetTag(new NbtCompound());

        return _compound!;
    }

    public void SetTag(NbtCompound? compound)
    {
        _compound = compound;

        if (Item.CanBeDepleted)
        {
            // Seems redundant but it ensures damage value is set in NBT tag
            DamageValue = DamageValue;
        }

        if (compound != null)
        {
            Item.VerifyTagAfterLoad(compound);
        }
    }
}