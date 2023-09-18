using System.Runtime.InteropServices;
using Mochi.Utils;

namespace MiaCrate.Client.Oshi.MacOs;

public static partial class SystemB
{
    
    private const string DllName = "/System/Library/Frameworks/System.framework/System";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">The ASCII name of the requested attribute.</param>
    /// <param name="oldp">A pointer to a buffer that receives the requested data.</param>
    /// <param name="oldlenp">A variable that contains the number of bytes in the buffer in the <paramref name="oldp"/> parameter.</param>
    /// <param name="newp">A pointer to a buffer that contains new data to assign to the attribute.</param>
    /// <param name="newlen">A variable that contains the size of the buffer in the <paramref name="newp"/> parameter, in bytes.</param>
    /// <returns><see cref="StdLibError.Ok"/> on success, or an error code that indicated a problem occurred.</returns>
    [LibraryImport(DllName, EntryPoint = "sysctlbyname", StringMarshalling = StringMarshalling.Utf8)]
    public static partial StdLibError SysCtlByName(string name, IntPtr oldp, ref nint oldlenp, IntPtr newp, nint newlen);
    
    [LibraryImport(DllName, EntryPoint = "mach_task_self")]
    public static partial int MachTaskSelf();

    [LibraryImport(DllName, EntryPoint = "mach_port_deallocate")]
    public static partial StdLibError MachPortDeallocate(int a, int b);

    public static string Sysctl(string name, string def)
    {
        nint size = 0;
        var errno = SysCtlByName(name, IntPtr.Zero, ref size, IntPtr.Zero, 0);
        if (errno != StdLibError.Ok)
        {
            Logger.Warn($"Failed sysctl call: {name}, Error code: {errno}");
            return def;
        }

        var ptr = Marshal.AllocHGlobal(size + 1);
        try
        {
            errno = SysCtlByName(name, ptr, ref size, IntPtr.Zero, 0);
            if (errno != StdLibError.Ok)
            {
                Logger.Warn($"Failed sysctl call: {name}, Error code: {errno}");
                return def;
            }

            return Marshal.PtrToStringAuto(ptr) ?? def;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
    
    public static ulong Sysctl(string name, ulong def)
    {
        nint size = sizeof(ulong);
        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            var errno = SysCtlByName(name, ptr, ref size, IntPtr.Zero, 0);
            if (errno != StdLibError.Ok)
            {
                Logger.Warn($"Failed sysctl call: {name}, Error code: {errno}");
                return def;
            }

            unsafe
            {
                var p = (ulong*)ptr;
                return *p;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
    
    public static uint Sysctl(string name, uint def)
    {
        nint size = sizeof(uint);
        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            var errno = SysCtlByName(name, ptr, ref size, IntPtr.Zero, 0);
            if (errno != StdLibError.Ok)
            {
                Logger.Warn($"Failed sysctl call: {name}, Error code: {errno}");
                return def;
            }

            unsafe
            {
                var p = (uint*)ptr;
                return *p;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}