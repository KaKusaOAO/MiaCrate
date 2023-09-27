namespace MiaCrate.Client.Utils;

public static class UnsafeUtil
{
    public static unsafe TOut* AsFixed<TOut>(byte[] buffer) where TOut : unmanaged
    {
        fixed (byte* buf = buffer)
        {
            return (TOut*) buf;
        }
    }
    
    public static unsafe TOut* AsFixed<T, TOut>(T[] buffer) where T : unmanaged where TOut : unmanaged
    {
        fixed (T* buf = buffer)
        {
            return (TOut*) buf;
        }
    }
}