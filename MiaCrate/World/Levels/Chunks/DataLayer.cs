namespace MiaCrate.World;

public class DataLayer
{
    public const int LayerCount = 16;
    public const int LayerSize = 128;
    public const int Size = LayerSize * LayerCount;
    private const int NibbleSize = 4;

    private byte[]? _data;
    private int _defaultValue;

    public DataLayer(int i = 0)
    {
        _defaultValue = i;
    }

    public DataLayer(byte[] data)
    {
        _data = data;
        _defaultValue = 0;

        if (data.Length != Size)
            throw new ArgumentException($"DataLayer should be {Size} bytes not: {data.Length}");
    }

    public int Get(int x, int y, int z) => Get(GetIndex(x, y, z));

    private int Get(int index)
    {
        if (_data == null) return _defaultValue;

        var j = GetByteIndex(index);
        var k = GetNibbleIndex(index);
        return _data[j] >> 4 * k & 15;
    }

    private static int GetNibbleIndex(int i) => i & 1;
    private static int GetByteIndex(int i) => i >> 1;

    private static int GetIndex(int x, int y, int z) => y << 8 | z << 4 | x;
}