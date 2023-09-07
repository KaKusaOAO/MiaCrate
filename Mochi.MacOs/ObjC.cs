using System.Runtime.InteropServices;

namespace Mochi.MacOs;

public static partial class ObjC
{
    private const string DllName = "objc.A";
    
    [LibraryImport(DllName, EntryPoint = "objc_lookUpClass", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr LookupClass(string name);

    [LibraryImport(DllName, EntryPoint = "class_getName", StringMarshalling = StringMarshalling.Utf8)]
    public static partial string GetClassName(IntPtr id);
    
    [LibraryImport(DllName, EntryPoint = "class_getProperty", StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr GetPropertyOfClass(IntPtr cls, string name);
    
    [LibraryImport(DllName, EntryPoint = "class_getSuperclass")]
    public static partial IntPtr GetSuperClassOfClass(IntPtr id);
    
    [DllImport(DllName, EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector, __arglist);
}

public class NSObject
{
    
}