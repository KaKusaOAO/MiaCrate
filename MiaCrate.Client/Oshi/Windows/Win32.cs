using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.System.SystemInformation;

namespace MiaCrate.Client.Oshi.Windows;

public static class Win32
{
    internal static SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX[] GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP type)
    {
        unsafe
        {
            var bufferSize = 1u;

            int err;
            do
            {
                var allocated = (SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX*) Marshal.AllocHGlobal((int) bufferSize);

                try
                {
                    if (PInvoke.GetLogicalProcessorInformationEx(type, allocated, ref bufferSize))
                    {
                        var list = new List<SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX>();

                        for (var offset = 0L; offset < bufferSize;)
                        {
                            var info = *allocated;
                            list.Add(info);
                            offset += info.Size;
                        }

                        return list.ToArray();
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal((IntPtr) allocated);
                }

                err = Marshal.GetLastWin32Error();
            } while (err == 122);

            throw new Win32Exception(err);
        }
    }
}