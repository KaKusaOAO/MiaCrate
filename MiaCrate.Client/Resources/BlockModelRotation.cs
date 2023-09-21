namespace MiaCrate.Client.Resources;

public sealed class BlockModelRotation : IModelState
{
    private const int Degrees = 360;
    
    private static readonly Dictionary<int, BlockModelRotation> _values = new();

    // ReSharper disable InconsistentNaming
    public static readonly BlockModelRotation X0_Y0 = new(0, 0);
    public static readonly BlockModelRotation X0_Y90 = new(0, 90);
    public static readonly BlockModelRotation X0_Y180 = new(0, 180);
    public static readonly BlockModelRotation X0_Y270 = new(0, 270);
    // ReSharper restore InconsistentNaming

    private static readonly Dictionary<int, BlockModelRotation> _byIndex = _values.ToDictionary(
        e => e.Value._index,
        e => e.Value);
        
    private readonly int _index;

    public int Ordinal { get; }

    public Transformation Transformation => throw new NotImplementedException();
    
    private BlockModelRotation(int x, int y)
    {
        _index = GetIndex(x, y);
        
        Ordinal = _values.Count;
        _values[Ordinal] = this;
    }

    private static int GetIndex(int x, int y) => x * Degrees + y;
    
    public static BlockModelRotation? By(int x, int y)
    {
        var index = GetIndex(Util.PositiveModulo(x, 360), Util.PositiveModulo(y, 360));
        return _byIndex.GetValueOrDefault(index);
    }
}