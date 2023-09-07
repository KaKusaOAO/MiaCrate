using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.System.SystemInformation;

namespace MiaCrate.Client.Oshi.Windows;

[SupportedOSPlatform("windows5.0")]
public static class VersionHelper
{
    public static bool IsWindowsVersionOrGreater(int major, int minor, int servicePackMajor)
    {
        var osvi = new OSVERSIONINFOEXW
        {
            dwOSVersionInfoSize = (uint) Marshal.SizeOf<OSVERSIONINFOEXW>(),
            dwMajorVersion = (uint) major,
            dwMinorVersion = (uint) minor,
            wServicePackMajor = (ushort) servicePackMajor
        };

        var dwlConditionMask = 0uL;
        dwlConditionMask = PInvoke.VerSetConditionMask(dwlConditionMask, VER_FLAGS.VER_MAJORVERSION, 3);
        dwlConditionMask = PInvoke.VerSetConditionMask(dwlConditionMask, VER_FLAGS.VER_MINORVERSION, 3);
        dwlConditionMask = PInvoke.VerSetConditionMask(dwlConditionMask, VER_FLAGS.VER_SERVICEPACKMAJOR, 3);

        var flags = VER_FLAGS.VER_MAJORVERSION | VER_FLAGS.VER_MINORVERSION | VER_FLAGS.VER_SERVICEPACKMAJOR;
        return PInvoke.VerifyVersionInfo(ref osvi, flags, dwlConditionMask);
    }

    public static bool IsWindows7OrGreater() => IsWindowsVersionOrGreater(6, 1, 0);
    public static bool IsWindows10OrGreater() => IsWindowsVersionOrGreater(10, 0, 0);
}