namespace MiaCrate;

public interface IEnumLike<T>
{
    public int Ordinal { get; }
    
    public static abstract T[] Values { get; }
}