namespace MiaCrate.World.Inventories;

public abstract class AbstractContainerMenu
{
    public const int SlotCLickedOutside = -999;
    public const int CarriedSlotSize = int.MaxValue;

    public enum QuickCraftType
    {
        Charitable,
        Greedy,
        Clone
    }

    public enum QuickCraftHeader
    {
        Start,
        Continue,
        End
    }
}