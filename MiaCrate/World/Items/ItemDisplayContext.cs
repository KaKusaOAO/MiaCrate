namespace MiaCrate.World.Items;

public sealed class ItemDisplayContext : IEnumLike<ItemDisplayContext>, IStringRepresentable
{
    private static readonly SortedDictionary<int, ItemDisplayContext> _values = new();

    public static ItemDisplayContext None { get; } = new(0, "none");
    public static ItemDisplayContext ThirdPersonLeftHand { get; } = new(1, "thirdperson_lefthand");
    public static ItemDisplayContext ThirdPersonRightHand { get; } = new(2, "thirdperson_righthand");
    public static ItemDisplayContext FirstPersonLeftHand { get; } = new(3, "firstperson_lefthand");
    public static ItemDisplayContext FirstPersonRightHand { get; } = new(4, "firstperson_righthand");
    public static ItemDisplayContext Head { get; } = new(5, "head");
    public static ItemDisplayContext Gui { get; } = new(6, "gui");
    public static ItemDisplayContext Ground { get; } = new(7, "ground");
    public static ItemDisplayContext Fixed { get; } = new(8, "fixed");
    
    public static ItemDisplayContext[] Values => _values.Values.ToArray();
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