using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using MiaCrate.Data;
using Mochi.Utils;

namespace MiaCrate.Client.Oshi.MacOs;

[SupportedOSPlatform("macos")]
public partial class MacCentralProcessor : AbstractCentralProcessor
{
    private const uint ArmCpuType  = 0x0100000c;
    private const uint M1CpuFamily = 0x1b588bb3;
    private const uint M2CpuFamily = 0xda33d83d;
    private const long DefaultFrequency = 2_400_000_000L;

    private static readonly Regex _cpuN = CreateCpuNRegex();

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
        var logicalProcessorCount = SystemB.Sysctl("hw.logicalcpu", 1);
        var physicalProcessorCount = SystemB.Sysctl("hw.physicalcpu", 1);
        var physicalPackageCount = SystemB.Sysctl("hw.packages", 1);
        var logProcs = new List<LogicalProcessor>();
        var pkgCoreKeys = new HashSet<int>();
        
        for (var i = 0; i < logicalProcessorCount; i++)
        {
            var coreId = (int) (i * physicalProcessorCount / logicalProcessorCount);
            var pkgId = (int) (i * physicalPackageCount / logicalProcessorCount);
            logProcs.Add(new LogicalProcessor(i, coreId, pkgId));
            pkgCoreKeys.Add((pkgId << 16) + coreId);
        }

        var compatMap = QueryCompatibleStrings();
        var perflevels = SystemB.Sysctl("hw.nperflevels", 1);
        var physProcs = pkgCoreKeys.Order().Select(k =>
        {
            var compat = compatMap.GetValueOrDefault(k, "").ToLowerInvariant();
            var efficiency = 0;
            
            if (compat.Contains("firestorm") || compat.Contains("avalanche"))
            {
                efficiency = 1;
            }

            return new PhysicalProcessor(k >> 16, k & 0xffff, efficiency, compat);
        }).ToList();

        return Pair.Of(logProcs, physProcs);
        // var caches = OrderedProcCaches(GetCacheValues((int) perflevels));
    }

    private static Dictionary<int, string> QueryCompatibleStrings()
    {
        var dict = new Dictionary<int, string>();

        var iter = IOKitUtil.GetMatchingServices("IOPlatformDevice");
        if (iter.Handle == 0) return dict;

        var cpu = IOKit.IOIteratorNext(iter);
        while (cpu.Handle != 0)
        {
            var namePtr = Marshal.AllocHGlobal(128);
            if (IOKit.IORegistryEntryGetName(cpu, namePtr) != 0)
                throw new Exception();

            var name = Marshal.PtrToStringUTF8(namePtr)!;
            Marshal.FreeHGlobal(namePtr);
            
            var matches = _cpuN.Matches(name.ToLowerInvariant());
            if (matches.Any())
            {
                if (!int.TryParse(matches[0].Groups[1].Value, out var procId))
                    procId = 0;

                var data = cpu.GetByteArray("compatible");
                if (data != null)
                    dict[procId] = Encoding.UTF8.GetString(data).Replace("\u0000", "");
            }
            
            cpu.Base.Dispose();
            cpu = IOKit.IOIteratorNext(iter);
        }
        
        iter.Base.Dispose();
        return dict;
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
            throw new NotSupportedException();
        }

        Util.LogFoobar();
        var cpuFreq = DefaultFrequency;
        var cpu64Bit = SystemB.Sysctl("hw.cpu64bit_capable", 0) != 0;
        return new ProcessorIdentifier(cpuVendor, cpuName, cpuFamily, cpuModel, cpuStepping, processorId, cpu64Bit,
            cpuFreq);;
    }

    private HashSet<ICentralProcessor.ProcessorCache> GetCacheValues(int perflevels)
    {
        var lineSize = (int) SystemB.Sysctl("hw.cachelinesize", 0L);
        var l1Associativity = (int) SystemB.Sysctl("machdep.cpu.cache.L1_associativity", 0, false);
        var l2Associativity = (int) SystemB.Sysctl("machdep.cpu.cache.L2_associativity", 0, false);
        var caches = new HashSet<ICentralProcessor.ProcessorCache>();
        
        for (var i = 0; i < perflevels; i++)
        {
            var size = SystemB.Sysctl($"hw.perflevel{i}.l1icachesize", 0, false);
            if (size > 0)
            {
                caches.Add(new ICentralProcessor.ProcessorCache(1, l1Associativity, lineSize, size,
                    ICentralProcessor.ProcessorCache.CacheType.Instruction));
            }
            
            size = SystemB.Sysctl($"hw.perflevel{i}.l1dcachesize", 0, false);
            if (size > 0)
            {
                caches.Add(new ICentralProcessor.ProcessorCache(1, l1Associativity, lineSize, size,
                    ICentralProcessor.ProcessorCache.CacheType.Data));
            }
            
            size = SystemB.Sysctl($"hw.perflevel{i}.l2cachesize", 0, false);
            if (size > 0)
            {
                caches.Add(new ICentralProcessor.ProcessorCache(2, l2Associativity, lineSize, size,
                    ICentralProcessor.ProcessorCache.CacheType.Unified));
            }
            
            size = SystemB.Sysctl($"hw.perflevel{i}.l3cachesize", 0, false);
            if (size > 0)
            {
                caches.Add(new ICentralProcessor.ProcessorCache(3, 0, lineSize, size,
                    ICentralProcessor.ProcessorCache.CacheType.Unified));
            }
        }

        return caches;
    }

    private static string GetPlatformExpert()
    {
        string? manufacturer = null;
        var platformExpert = IOKitUtil.GetMatchingService("IOPlatformExpertDevice");
        if (platformExpert.Handle != 0)
        {
            Util.LogFoobar();
        }

        return "Apple Inc.";
    }

    private bool InternalCheckIsArmCpu() =>
        PhysicalProcessors.Select(p => p.Efficiency).Any(e => e > 0);
    
    [GeneratedRegex("^cpu(\\d+)$")]
    private static partial Regex CreateCpuNRegex();
}