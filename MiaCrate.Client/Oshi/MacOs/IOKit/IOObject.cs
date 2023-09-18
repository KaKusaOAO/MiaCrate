using System.Runtime.InteropServices;

namespace MiaCrate.Client.Oshi.MacOs;

[StructLayout(LayoutKind.Sequential)]
public struct IOObject : IDisposable
{
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
}

[StructLayout(LayoutKind.Explicit)]
public struct IOService
{
    [FieldOffset(0)]
    public IntPtr Handle;
    
    [FieldOffset(0)]
    public IOObject Base;
    
    [FieldOffset(0)]
    public IORegistryEntry Entry;
}