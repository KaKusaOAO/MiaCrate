using System.Management;
using System.Runtime.Versioning;
using Windows.Win32.System.SystemInformation;
using MiaCrate.Data;
using EnumerationOptions = System.Management.EnumerationOptions;

namespace MiaCrate.Client.Oshi.Windows;

[SupportedOSPlatform("windows5.0")]
public static class LogicalProcessorInformation
{
    private static readonly bool _isWin10OrGreater = VersionHelper.IsWindows10OrGreater();
    
    public static IPair<List<LogicalProcessor>, List<PhysicalProcessor>> GetLogicalProcessorInformationEx()
    {
        var procInfo = Win32.GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationAll);
        var packages = new List<GROUP_AFFINITY[]>();
        var cores = new List<GROUP_AFFINITY>();
        var numaNodes = new List<NUMA_NODE_RELATIONSHIP>();
        var coreEfficiencyMap = new Dictionary<GROUP_AFFINITY, int>();

        for (var pkg = 0; pkg < procInfo.Length; pkg++)
        {
            var info = procInfo[pkg];
            switch (info.Relationship)
            {
                case LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore:
                    var core = info.Anonymous.Processor;
                    var groupMasks = core.GroupMask.AsSpan().ToArray();
                    cores.Add(groupMasks.First());

                    if (_isWin10OrGreater)
                    {
                        coreEfficiencyMap[groupMasks.First()] = core.EfficiencyClass;
                    }

                    break;
                case LOGICAL_PROCESSOR_RELATIONSHIP.RelationNumaNode:
                    numaNodes.Add(info.Anonymous.NumaNode);
                    break;
                case LOGICAL_PROCESSOR_RELATIONSHIP.RelationCache:
                    break;
                case LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorPackage:
                    packages.Add(info.Anonymous.Processor.GroupMask.AsSpan().ToArray());
                    break;
                default:
                    break;
            }
        }

        cores = cores
            .OrderBy(c => c.Group * 64 + Util.NumberOfTrailingZeros(c.Mask))
            .ToList();
        packages = packages
            .OrderBy(p => p[0].Group * 64 + Util.NumberOfTrailingZeros(p[0].Mask))
            .ToList();
        numaNodes = numaNodes
            .OrderBy(n => n.NodeNumber).ToList();

        var processorIdMap = new Dictionary<int, string>();
        var result = new ManagementObjectSearcher("root\\cimv2", "SELECT * FROM Win32_Processor", new EnumerationOptions
        {
            ReturnImmediately = true
        }).Get();

        {
            var pkg = 0;
            foreach (var item in result)
            {
                processorIdMap.Add(pkg++, item.GetPropertyValue("ProcessorId").ToString()!);
            }
        }

        var logProcs = new List<LogicalProcessor>();
        var corePkgMap = new Dictionary<int, int>();
        var pkgCpuidMap = new Dictionary<int, string>();

        foreach (var node in numaNodes)
        {
            var nodeNum = node.NodeNumber;
            var group = node.Anonymous.GroupMask.Group;
            var mask = node.Anonymous.GroupMask.Mask;
            var lowBit = Util.NumberOfTrailingZeros(mask);
            var hiBit = 63 - Util.NumberOfTrailingZeros(mask);

            for (var lp = lowBit; lp <= hiBit; lp++)
            {
                if ((mask & 1uL << lp) != 0)
                {
                    var coreId = GetMatchingCore(cores, group, lp);
                    var pkgId = GetMatchingPackage(packages, group, lp);
                    corePkgMap[coreId] = pkgId;
                    pkgCpuidMap[coreId] = processorIdMap.GetValueOrDefault(pkgId, "");

                    var logProc = new LogicalProcessor(lp, coreId, pkgId, (int) nodeNum, group);
                    logProcs.Add(logProc);
                }
            }
        }

        var physProcs = GetPhysProcs(cores, coreEfficiencyMap, corePkgMap, pkgCpuidMap);
        return Pair.Of(logProcs, physProcs);
    }

    private static List<PhysicalProcessor> GetPhysProcs(
        List<GROUP_AFFINITY> cores,
        Dictionary<GROUP_AFFINITY, int> coreEfficiencyMap,
        Dictionary<int, int> corePkgMap,
        Dictionary<int, string> coreCpuidMap)
    {
        var result = new List<PhysicalProcessor>();

        for (var coreId = 0; coreId < cores.Count; coreId++)
        {
            var efficiency = coreEfficiencyMap.GetValueOrDefault(cores[coreId], 0);
            var cpuid = coreCpuidMap.GetValueOrDefault(coreId, "");
            var pkgId = corePkgMap.GetValueOrDefault(coreId, 0);
            result.Add(new PhysicalProcessor(pkgId, coreId, efficiency, cpuid));
        }

        return result;
    }

    private static int GetMatchingCore(List<GROUP_AFFINITY> cores, int g, int lp)
    {
        unsafe
        {
            for (var j = 0; j < cores.Count; j++)
            {
                if ((cores[j].Mask & 1uL << lp) != 0L && cores[j].Group == g) return j;
            }
        }

        return 0;
    }

    private static int GetMatchingPackage(List<GROUP_AFFINITY[]> packages, int g, int lp)
    {
        unsafe
        {
            for (var i = 0; i < packages.Count; i++)
            {
                var package = packages[i];
                
                for (var j = 0; j < packages[i].Length; j++)
                {
                    var item = package[j];
                    if ((item.Mask & 1uL << lp) != 0L && item.Group == g) return i;
                }
            }
        }

        return 0;
    }
}