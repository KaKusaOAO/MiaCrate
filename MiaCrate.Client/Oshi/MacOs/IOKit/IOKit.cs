using System.Runtime.InteropServices;

namespace MiaCrate.Client.Oshi.MacOs;

public static partial class IOKit
{
    private const string DllName = "/System/Library/Frameworks/IOKit.framework/IOKit";

    [LibraryImport(DllName)]
    public static partial int IOMasterPort(int v, out int result);
    
    [LibraryImport(DllName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial CFMutableDictionaryRef IOServiceMatching(string name);
    
    [LibraryImport(DllName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial CFMutableDictionaryRef IOServiceNameMatching(string name);

    [LibraryImport(DllName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial CFMutableDictionaryRef IOBSDMatching(int a, int b, string name);

    [LibraryImport(DllName)]
    public static partial int IOObjectRelease(IOObject obj);

    [LibraryImport(DllName)]
    public static partial int IORegistryEntryGetName(IORegistryEntry entry, IntPtr ptr);
    
    [LibraryImport(DllName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int IORegistryEntryGetChildIterator(IORegistryEntry entry, string ptr, out IntPtr result);
    
    [LibraryImport(DllName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int IORegistryEntryGetChildEntry(IORegistryEntry entry, string ptr, out IntPtr result);
    
    [LibraryImport(DllName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int IORegistryEntryGetParentEntry(IORegistryEntry entry, string ptr, out IntPtr result);
    
    [LibraryImport(DllName)]
    public static partial IOService IOServiceGetMatchingService(int v, CFDictionaryRef dict);
    
    [LibraryImport(DllName)]
    public static partial StdLibError IOServiceGetMatchingServices(int v, CFDictionaryRef dict, out IOIterator iterator);
    
    [LibraryImport(DllName)]
    public static partial IORegistryEntry IOIteratorNext(IOIterator iterator);

    [LibraryImport(DllName)]
    public static partial IORegistryEntry IORegistryGetRootEntry(int v);
    
    [LibraryImport(DllName)]
    public static partial CFTypeRef IORegistryEntryCreateCFProperty(IORegistryEntry entry, CFStringRef key, CFAllocatorRef allocator, int v);
}