namespace MiaCrate.World.Inventories;

public class InventoryMenu : RecipeBookMenu<ICraftingContainer>
{
    public const int ContainerId = 0;
    public const int ResultSlot = 0;
    public const int CraftSlotStart = ResultSlot + 1;
    public const int CraftSlotEnd = CraftSlotStart + 4;
    public const int ArmorSlotStart = CraftSlotEnd;
    public const int ArmorSlotEnd = ArmorSlotStart + 4;
    public const int InvSlotStart = ArmorSlotEnd;
    public const int InvSlotEnd = InvSlotStart + 9 * 3;
    public const int UseRowSlotStart = InvSlotEnd;
    public const int UseRowSlotEnd = UseRowSlotStart + 9;
    public const int ShieldSlot = UseRowSlotEnd;
    
    public static readonly ResourceLocation BlockAtlas = new("textures/atlas/blocks.png");
}