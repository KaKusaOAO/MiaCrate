using System.Runtime.InteropServices;

namespace MiaCrate.Client.Oshi.MacOs;

[StructLayout(LayoutKind.Sequential)]
public struct IOObject : IDisposable
{
    public static IOObject Null { get; } = new();
    
    public IntPtr Handle;

    public void Dispose()
    {
        IOKit.IOObjectRelease(this);
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct IORegistryEntry
{
    [FieldOffset(0)]
    public IntPtr Handle;
    
    [FieldOffset(0)]
    public IOObject Base;

    public byte[]? GetByteArray(string key)
    {
        var k = CFStringRef.Create(key);
        var prop = CreateCFProperty(k);
        k.Type.Release();
        if (prop == CFTypeRef.Null) return null;

        var data = new CFDataRef(prop.Handle);
        var len = data.Length;
        var ptr = data.BytePointer;

        var arr = new byte[len];
        Marshal.Copy(ptr, arr, 0, len);
        return arr;
    }

    public CFTypeRef CreateCFProperty(CFStringRef key) => 
        IOKit.IORegistryEntryCreateCFProperty(this, key, CFAllocatorRef.Default, 0);
}

[StructLayout(LayoutKind.Explicit)]
public struct IOService
{
    public static IOService Null { get; } = new();
    
    [FieldOffset(0)]
    public IntPtr Handle;
    
    [FieldOffset(0)]
    public IOObject Base;
    
    [FieldOffset(0)]
    public IORegistryEntry Entry;
}

[StructLayout(LayoutKind.Explicit)]
public struct IOIterator
{
    public static IOIterator Null { get; } = new();
    
    [FieldOffset(0)]
    public IntPtr Handle;
    
    [FieldOffset(0)]
    public IOObject Base;
}