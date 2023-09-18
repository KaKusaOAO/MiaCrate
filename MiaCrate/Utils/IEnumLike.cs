namespace MiaCrate;

public interface IEnumLike<T>
{
    public int Ordinal { get; }
}

public static class EnumLike
{
    public static IEnumLike<T> FromStruct<T>(T val) where T : struct, Enum => new StructEnum<T>(val);

    private class StructEnum<T> : IEnumLike<T> where T : struct, Enum
    {
        private readonly T _value;

        public StructEnum(T value)
        {
            _value = value;
        }

        public int Ordinal => Convert.ToInt32(_value);
    }
}

