using System.Runtime.Versioning;

namespace MiaCrate.Client.Oshi.Windows;

[SupportedOSPlatform("windows")]
public class WindowsHardware : IHardware
{
    public ICentralProcessor Processor => new WindowsCentralProcessor();
}