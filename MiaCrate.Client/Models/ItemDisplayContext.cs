namespace MiaCrate.Client.Models;

public sealed class ItemDisplayContext : IEnumLike<ItemDisplayContext>, IStringRepresentable
{
    private static readonly SortedDictionary<int, ItemDisplayContext> _values = new();

    public static readonly ItemDisplayContext None = new(0, "none");
    public static readonly ItemDisplayContext ThirdPersonLeftHand = new(1, "thirdperson_lefthand");
    public static readonly ItemDisplayContext ThirdPersonRightHand = new(2, "thirdperson_righthand");
    public static readonly ItemDisplayContext FirstPersonLeftHand = new(3, "firstperson_lefthand");
    public static readonly ItemDisplayContext FirstPersonRightHand = new(4, "firstperson_righthand");
    public static readonly ItemDisplayContext Head = new(5, "head");
    public static readonly ItemDisplayContext Gui = new(6, "gui");
    public static readonly ItemDisplayContext Ground = new(7, "ground");
    public static readonly ItemDisplayContext Fixed = new(8, "fixed");
    
    public byte Id { get; }
    public int Ordinal { get; }
    public string SerializedName { get; }
    public bool IsFirstPerson => this == FirstPersonLeftHand || this == FirstPersonRightHand;

    private ItemDisplayContext(int id, string name)
    {
        Id = (byte) id;
        SerializedName = name;

        var ordinal = _values.Count;
        Ordinal = ordinal;
        _values[ordinal] = this;
    }
}