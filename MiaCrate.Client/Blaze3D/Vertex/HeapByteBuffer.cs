using System.Runtime.InteropServices;

namespace MiaCrate.Client;

public class HeapByteBuffer : ByteBuffer
{
    private int _capacity;
    private byte[] _buffer;
    
    public override int Position { get; protected set; }
    public override int Limit { get; protected set; }

    public override int Capacity => _capacity;

    public HeapByteBuffer(int capacity)
    {
        _capacity = capacity;
        _buffer = new byte[capacity];
    }

    public override void Put(int index, byte[] arr)
    {
        if (arr.Length + index > Capacity)
            throw new IndexOutOfRangeException();

        Array.Copy(arr, 0, _buffer, index, arr.Length);
    }

    public override void Put(byte[] arr)
    {
        if (arr.Length + Position > Capacity)
            throw new IndexOutOfRangeException();

        Array.Copy(arr, 0, _buffer, Position, arr.Length);
        Position += arr.Length;
    }

    public override void Resize(int capacity)
    {
        Array.Resize(ref _buffer, capacity);
        _capacity = capacity;
        Clear();
    }
}

public class DirectByteBuffer : ByteBuffer, IDisposable
{
    private int _capacity;
    private IntPtr _buffer;

    public override int Position { get; protected set; }
    public override int Limit { get; protected set; }

    public override int Capacity => _capacity;
    
    public DirectByteBuffer(int capacity)
    {
        _capacity = capacity;
        _buffer = Marshal.AllocHGlobal(capacity);
    }

    public override void Put(int index, byte[] arr)
    {
        if (arr.Length + index > Capacity)
            throw new IndexOutOfRangeException();

        Marshal.Copy(arr, 0, _buffer + index, arr.Length);
    }

    public override void Put(byte[] arr)
    {
        if (arr.Length + Position > Capacity)
            throw new IndexOutOfRangeException();

        Marshal.Copy(arr, 0, _buffer + Position, arr.Length);
        Position += arr.Length;
    }

    public override void Resize(int capacity)
    {
        var resized = Marshal.ReAllocHGlobal(_buffer, capacity);
        if (resized == IntPtr.Zero)
            throw new OutOfMemoryException();

        _capacity = capacity;
        _buffer = resized;
        Clear();
    }
    
    public void Dispose()
    {
        if (_buffer == IntPtr.Zero) return;
        Marshal.FreeHGlobal(_buffer);
        _buffer = IntPtr.Zero;
    }
}