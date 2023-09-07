using System.Runtime.InteropServices;
using MiaCrate.Client.Oshi.Windows;

namespace MiaCrate.Client.Oshi;

public class SystemInfo
{
    private readonly Lazy<IHardware> _hardware = new(CreateHardware);

    public IHardware Hardware => _hardware.Value;
    
    private static IHardware CreateHardware()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return new WindowsHardware();
        throw new PlatformNotSupportedException("Operating system not supported");
    }
}