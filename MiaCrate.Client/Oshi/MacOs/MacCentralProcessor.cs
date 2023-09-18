using System.Runtime.Versioning;
using MiaCrate.Data;
using Mochi.Utils;

namespace MiaCrate.Client.Oshi.MacOs;

[SupportedOSPlatform("macos")]
public class MacCentralProcessor : AbstractCentralProcessor
{
    private const uint ArmCpuType  = 0x0100000c;
    private const uint M1CpuFamily = 0x1b588bb3;
    private const uint M2CpuFamily = 0xda33d83d;
    private const long DefaultFrequency = 2_400_000_000L;

    // Equivalents of hw.cpufrequency on Apple Silicon, defaulting to Rosetta value
    // Will update during initialization
    private long _performanceCoreFrequency = DefaultFrequency;
    private long _efficiencyCoreFrequency = DefaultFrequency;
    private readonly Lazy<string> _vendor = new(GetPlatformExpert);

    public bool IsArmCpu { get; }

    internal MacCentralProcessor()
    {
        IsArmCpu = InternalCheckIsArmCpu();
    }

    protected override IPair<List<LogicalProcessor>, List<PhysicalProcessor>> InitProcessorCounts()
    {
        var str = _vendor.Value;
        throw new NotImplementedException();
    }

    protected override ProcessorIdentifier QueryProcessorId()
    {
        var cpuName = SystemB.Sysctl("machdep.cpu.brand_string", "");
        string cpuVendor;
        string cpuStepping;
        string cpuModel;
        string cpuFamily;
        string processorId;
        
        // Initial M1 chips said "Apple Processor". Later branding includes M1, M1 Pro,
        // M1 Max, M2, etc. So if it starts with Apple it's M-something.
        if (cpuName.StartsWith("Apple"))
        {
            // Processing an M1 chip
            cpuVendor = _vendor.Value;
            cpuStepping = "0"; // No correlation yet
            cpuModel = "0"; // No correlation yet

            uint type;
            uint family;

            if (IsArmCpu)
            {
                type = ArmCpuType;
                family = cpuName.Contains("M2") ? M2CpuFamily : M1CpuFamily;
            }
            else
            {
                type = SystemB.Sysctl("hw.cputype", 0);
                family = SystemB.Sysctl("hw.cpufamily", 0);
            }
            // Translate to output
            cpuFamily = $"0x{family:x8}";
            // Processor ID is an intel concept but CPU type + family conveys same info
            processorId = $"{type:x8}x{family:x8}";
        }
        else
        {
            // Processing an Intel chip
            
        }

        throw new NotImplementedException();
    }

    private static string GetPlatformExpert()
    {
        string? manufacturer = null;
        var platformExpert = IOKitUtil.GetMatchingService("IOPlatformExpertDevice");
        if (platformExpert.Handle != 0)
        {
            Logger.Info("Test");
        }

        return "Apple Inc.";
    }

    private bool InternalCheckIsArmCpu() =>
        PhysicalProcessors.Select(p => p.Efficiency).Any(e => e > 0);
}