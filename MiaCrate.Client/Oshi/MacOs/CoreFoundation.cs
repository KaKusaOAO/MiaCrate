using System.Runtime.InteropServices;

namespace MiaCrate.Client.Oshi.MacOs;

public static partial class CoreFoundation
{
    private const string DllName = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    [LibraryImport(DllName)]
    public static partial CFTypeRef CFRetain(CFTypeRef ptr);

    [LibraryImport(DllName)]
    public static partial IntPtr CFDictionaryGetValue(CFDictionaryRef dict, IntPtr key);
    
    [LibraryImport(DllName)]
    public static partial IntPtr CFDictionarySetValue(CFMutableDictionaryRef dict, IntPtr key, IntPtr value);
}

[StructLayout(LayoutKind.Sequential)]
public struct CFIndex
{
    public long Value;
}

[StructLayout(LayoutKind.Sequential)]
public struct CFTypeRef
{
    public IntPtr Handle;
}

[StructLayout(LayoutKind.Explicit)]
public struct CFDictionaryRef
{
    [FieldOffset(0)]
    public CFTypeRef Type;

    [FieldOffset(0)]
    public IntPtr Handle;
}

[StructLayout(LayoutKind.Explicit)]
public struct CFMutableDictionaryRef
{
    [FieldOffset(0)]
    public CFDictionaryRef Dictionary;

    [FieldOffset(0)]
    public CFTypeRef Type;

    [FieldOffset(0)]
    public IntPtr Handle;
}