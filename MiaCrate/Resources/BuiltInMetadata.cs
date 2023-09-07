namespace MiaCrate.Resources;

public class BuiltInMetadata
{
    private static readonly BuiltInMetadata _empty = new(new Dictionary<IMetadataSectionSerializer, object>());
    private readonly Dictionary<IMetadataSectionSerializer, object> _values;

    private BuiltInMetadata(Dictionary<IMetadataSectionSerializer, object> map)
    {
        _values = map;
    }

    public static BuiltInMetadata Of() => _empty;

    public static BuiltInMetadata Of<T>(IMetadataSectionSerializer<T> serializer, T obj)
    {
        return new BuiltInMetadata(new Dictionary<IMetadataSectionSerializer, object>
        {
            [serializer] = obj!
        });
    }

    public static BuiltInMetadata Of<T1, T2>(
        IMetadataSectionSerializer<T1> serializer1, T1 obj1,
        IMetadataSectionSerializer<T2> serializer2, T2 obj2)
    {
        return new BuiltInMetadata(new Dictionary<IMetadataSectionSerializer, object>
        {
            [serializer1] = obj1!,
            [serializer2] = obj2!
        });
    }
}