using System.Runtime.Versioning;

namespace MiaCrate.Client.Oshi.MacOs;

[SupportedOSPlatform("macos")]
public class MacHardware : IHardware
{
    public ICentralProcessor Processor { get; } = new MacCentralProcessor();
}