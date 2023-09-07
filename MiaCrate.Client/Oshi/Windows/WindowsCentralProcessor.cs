using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Windows.Win32;
using Windows.Win32.System.SystemInformation;
using MiaCrate.Client.Utils;
using MiaCrate.Data;
using Microsoft.Win32;
using EnumerationOptions = System.Management.EnumerationOptions;

namespace MiaCrate.Client.Oshi.Windows;

[SupportedOSPlatform("windows5.1.2600")]
public class WindowsCentralProcessor : AbstractCentralProcessor
{
    private Dictionary<string, int> _numaNodeProcToLogicalProcMap;

    protected override IPair<List<LogicalProcessor>, List<PhysicalProcessor>> InitProcessorCounts()
    {
        if (VersionHelper.IsWindows7OrGreater())
        {
            var procs = LogicalProcessorInformation.GetLogicalProcessorInformationEx();
            var curNode = -1;
            var procNum = 0;
            var lp = 0;

            _numaNodeProcToLogicalProcMap = new Dictionary<string, int>();

            foreach (var logProc in procs.First!)
            {
                var node = logProc.NumaNode;
                if (node != curNode)
                {
                    curNode = node;
                    procNum = 0;
                }

                var key = $"{logProc.NumaNode}, {procNum++}";
                _numaNodeProcToLogicalProcMap[key] = lp++;
            }

            return procs;
        }

        throw new NotSupportedException("Too old lol");
    }

    protected override ProcessorIdentifier QueryProcessorId()
    {
        var cpuVendor = "";
        var cpuName = "";
        var cpuIdentifier = "";
        var cpuFamily = "";
        var cpuModel = "";
        var cpuStepping = "";
        var cpuVendorFreq = 0L;
        var cpu64bit = false;

        const string cpuRegistryRoot = @"HARDWARE\DESCRIPTION\System\CentralProcessor";
        var root = Registry.LocalMachine.OpenSubKey(cpuRegistryRoot)!;
        var ids = root.GetSubKeyNames();
        if (ids.Any())
        {
            var path = ids[0];
            var info = root.OpenSubKey(path)!;
            cpuVendor = info.GetValue("VendorIdentifier")!.ToString();
            cpuName = info.GetValue("ProcessorNameString")!.ToString();
            cpuIdentifier = info.GetValue("Identifier")!.ToString();

            try
            {
                var freq = info.GetValue("~MHz");
                if (freq != null)
                {
                    cpuVendorFreq = (int)freq * 1000000L;
                }
            }
            catch
            {
                // ...
            }
        }

        if (!string.IsNullOrEmpty(cpuIdentifier))
        {
            cpuFamily = ParseIdentifier(cpuIdentifier, "Family");
            cpuModel = ParseIdentifier(cpuIdentifier, "Model");
            cpuStepping = ParseIdentifier(cpuIdentifier, "Stepping");
        }

        try
        {
            PInvoke.GetNativeSystemInfo(out var sysInfo);
            var arch = sysInfo.Anonymous.Anonymous.wProcessorArchitecture;
            if (arch is PROCESSOR_ARCHITECTURE.PROCESSOR_ARCHITECTURE_IA64
                or PROCESSOR_ARCHITECTURE.PROCESSOR_ARCHITECTURE_AMD64
                or PROCESSOR_ARCHITECTURE.PROCESSOR_ARCHITECTURE_ARM64)
                cpu64bit = true;
        }
        catch
        {
            // ...   
        }
        
        var result = new ManagementObjectSearcher("root\\cimv2", "SELECT * FROM Win32_Processor", new EnumerationOptions
        {
            ReturnImmediately = true
        }).Get();

        string processorId;
        if (result.Count > 0)
        {
            var iterator = result.GetEnumerator();
            iterator.MoveNext();
            processorId = iterator.Current.GetPropertyValue("ProcessorId").ToString()!;
            iterator.Dispose();
        }
        else
        {
            processorId = CreateProcessorId(cpuStepping, cpuModel, cpuFamily,
                cpu64bit ? new[] {"ia64"} : Array.Empty<string>());
        }
        
        return new ProcessorIdentifier(cpuVendor!, cpuName!, cpuFamily, cpuModel, cpuStepping, processorId, cpu64bit, cpuVendorFreq);
    }

    protected static string CreateProcessorId(string stepping, string model, string family, string[] flags)
    {
        var processorIdBytes = 0L;
        var steppingL = ParseUtils.ParseLongOrDefault(stepping, 0L);
        var modelL = ParseUtils.ParseLongOrDefault(model, 0L);
        var familyL = ParseUtils.ParseLongOrDefault(family, 0L);
        processorIdBytes |= steppingL & 15L;
        processorIdBytes |= (modelL & 15L) << 4;
        processorIdBytes |= (modelL & 240L) << 16;
        processorIdBytes |= (familyL & 15L) << 8;
        processorIdBytes |= (familyL & 240L) << 20;
        var hwcap = 0L;
        /*
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            hwcap = (Long)Auxv.queryAuxv().getOrDefault(16, 0L);
        }
        */

        if (hwcap > 0L) {
            processorIdBytes |= hwcap << 32;
        } else {
            var var14 = flags;
            var var15 = flags.Length;

            for(var var16 = 0; var16 < var15; ++var16) {
                switch (var14[var16]) {
                    case "fpu":
                        processorIdBytes |= 4294967296L;
                        break;
                    case "vme":
                        processorIdBytes |= 8589934592L;
                        break;
                    case "de":
                        processorIdBytes |= 17179869184L;
                        break;
                    case "pse":
                        processorIdBytes |= 34359738368L;
                        break;
                    case "tsc":
                        processorIdBytes |= 68719476736L;
                        break;
                    case "msr":
                        processorIdBytes |= 137438953472L;
                        break;
                    case "pae":
                        processorIdBytes |= 274877906944L;
                        break;
                    case "mce":
                        processorIdBytes |= 549755813888L;
                        break;
                    case "cx8":
                        processorIdBytes |= 1099511627776L;
                        break;
                    case "apic":
                        processorIdBytes |= 2199023255552L;
                        break;
                    case "sep":
                        processorIdBytes |= 8796093022208L;
                        break;
                    case "mtrr":
                        processorIdBytes |= 17592186044416L;
                        break;
                    case "pge":
                        processorIdBytes |= 35184372088832L;
                        break;
                    case "mca":
                        processorIdBytes |= 70368744177664L;
                        break;
                    case "cmov":
                        processorIdBytes |= 140737488355328L;
                        break;
                    case "pat":
                        processorIdBytes |= 281474976710656L;
                        break;
                    case "pse-36":
                        processorIdBytes |= 562949953421312L;
                        break;
                    case "psn":
                        processorIdBytes |= 1125899906842624L;
                        break;
                    case "clfsh":
                        processorIdBytes |= 2251799813685248L;
                        break;
                    case "ds":
                        processorIdBytes |= 9007199254740992L;
                        break;
                    case "acpi":
                        processorIdBytes |= 18014398509481984L;
                        break;
                    case "mmx":
                        processorIdBytes |= 36028797018963968L;
                        break;
                    case "fxsr":
                        processorIdBytes |= 72057594037927936L;
                        break;
                    case "sse":
                        processorIdBytes |= 144115188075855872L;
                        break;
                    case "sse2":
                        processorIdBytes |= 288230376151711744L;
                        break;
                    case "ss":
                        processorIdBytes |= 576460752303423488L;
                        break;
                    case "htt":
                        processorIdBytes |= 1152921504606846976L;
                        break;
                    case "tm":
                        processorIdBytes |= 2305843009213693952L;
                        break;
                    case "ia64":
                        processorIdBytes |= 4611686018427387904L;
                        break;
                    case "pbe":
                        processorIdBytes |= long.MinValue;
                        break;
                }
            }
        }

        return $"{processorIdBytes:x16}";
    }

    private static string ParseIdentifier(string identifier, string key)
    {
        var idSplit = Regex.Split(identifier, "\\s+");
        var found = false;

        foreach (var s in idSplit)
        {
            if (found) return s;
            found = s == key;
        }

        return "";
    }
}