using System.Runtime.InteropServices;

namespace MiaCrate.Client.Oshi.MacOs;

public static partial class CoreFoundation
{
    private const string DllName = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    [LibraryImport(DllName)]
    public static partial CFTypeRef CFRetain(CFTypeRef ptr);

    [LibraryImport(DllName)]
    public static partial CFTypeRef CFRelease(CFTypeRef ptr);

    [LibraryImport(DllName)]
    public static partial IntPtr CFDictionaryGetValue(CFDictionaryRef dict, IntPtr key);
    
    [LibraryImport(DllName)]
    public static partial IntPtr CFDictionarySetValue(CFMutableDictionaryRef dict, IntPtr key, IntPtr value);

    [LibraryImport(DllName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial CFStringRef CFStringCreateWithCharacters(CFAllocatorRef allocator, string str, CFIndex index);

    [LibraryImport(DllName)]
    public static partial CFAllocatorRef CFAllocatorGetDefault();
    
    
    [LibraryImport(DllName)]
    public static partial CFIndex CFDataGetLength(CFDataRef data);
    
    [LibraryImport(DllName)]
    public static partial IntPtr CFDataGetBytePtr(CFDataRef data);
}

[StructLayout(LayoutKind.Sequential)]
public struct CFIndex
{
    public long Value;

    public CFIndex(long value)
    {
        Value = value;
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct CFDataRef
{
    [FieldOffset(0)]
    public CFTypeRef Type;

    [FieldOffset(0)]
    public IntPtr Handle;

    public int Length => (int) CoreFoundation.CFDataGetLength(this).Value;
    public IntPtr BytePointer => CoreFoundation.CFDataGetBytePtr(this);

    public CFDataRef(IntPtr handle)
    {
        Handle = handle;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct CFTypeRef
{
    public static CFTypeRef Null { get; } = new();
    
    public IntPtr Handle;

    public void Release() => CoreFoundation.CFRelease(this);

    public static bool operator ==(CFTypeRef a, CFTypeRef b) => a.Handle == b.Handle;
    public static bool operator !=(CFTypeRef a, CFTypeRef b) => a.Handle != b.Handle;
    public static bool operator ==(CFTypeRef? a, CFTypeRef? b)
    {
        if (!a.HasValue)
            return !b.HasValue || b.Value.Handle == 0;

        return b.HasValue && b.Value.Handle == a.Value.Handle;
    }

    public static bool operator !=(CFTypeRef? a, CFTypeRef? b) => !(a == b);
}

[StructLayout(LayoutKind.Explicit)]
public struct CFStringRef
{
    [FieldOffset(0)]
    public CFTypeRef Type;

    [FieldOffset(0)]
    public IntPtr Handle;

    public static CFStringRef Create(string str) =>
        CoreFoundation.CFStringCreateWithCharacters(CFAllocatorRef.Null, str, new CFIndex(str.Length));
}

[StructLayout(LayoutKind.Explicit)]
public struct CFAllocatorRef
{
    public static CFAllocatorRef Null { get; } = new();
    public static CFAllocatorRef Default => CoreFoundation.CFAllocatorGetDefault();
    
    [FieldOffset(0)]
    public CFTypeRef Type;

    [FieldOffset(0)]
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