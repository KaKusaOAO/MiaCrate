using System.Diagnostics.CodeAnalysis;
using MiaCrate.Data;

namespace MiaCrate.Client.Oshi;

public abstract class AbstractCentralProcessor : ICentralProcessor
{
    private readonly Lazy<ProcessorIdentifier> _cpuid;
    
    public ProcessorIdentifier ProcessorIdentifier => _cpuid.Value;

    public long MaxFrequency => throw new NotImplementedException();

    public long[] CurrentFrequency => throw new NotImplementedException();

    public List<LogicalProcessor> LogicalProcessors { get; }

    public List<PhysicalProcessor> PhysicalProcessors { get; }

    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected AbstractCentralProcessor()
    {
        _cpuid = new Lazy<ProcessorIdentifier>(QueryProcessorId);
        
        var pair = InitProcessorCounts();
        LogicalProcessors = pair.First!;

        if (pair.Second == null)
        {
            var pkgCoreKeys = LogicalProcessors
                .Select(p => p.PhysicalPackageNumber << 16 + p.PhysicalProcessorNumber).ToHashSet();
            PhysicalProcessors = pkgCoreKeys.Order()
                .Select(k => new PhysicalProcessor(k >> 16, k & 0xffff))
                .ToList();
        }
        else
        {
            PhysicalProcessors = pair.Second;
        }

        var physPkgs = new HashSet<int>();
        foreach (var logProc in LogicalProcessors)
        {
            var pkg = logProc.PhysicalPackageNumber;
            physPkgs.Add(pkg);
        }

        LogicalProcessorCount = LogicalProcessors.Count;
        PhysicalProcessorCount = PhysicalProcessors.Count;
    }

    protected abstract IPair<List<LogicalProcessor>, List<PhysicalProcessor>> InitProcessorCounts();

    protected abstract ProcessorIdentifier QueryProcessorId();

    public double GetSystemCpuLoadBetweenTicks(long[] result)
    {
        throw new NotImplementedException();
    }

    public long[] GetSystemCpuLoadTicks()
    {
        throw new NotImplementedException();
    }

    public int LogicalProcessorCount { get; }

    public int PhysicalProcessorCount { get; }

    public double[] GetSystemLoadAverage()
    {
        throw new NotImplementedException();
    }

    public static List<ICentralProcessor.ProcessorCache> OrderedProcCaches(
        IEnumerable<ICentralProcessor.ProcessorCache> caches)
    {
        return caches.OrderBy(c =>
                -1000 * c.Level + 100 * (int)c.Type - HighestOneBit(c.CacheSize))
            .ToList();
    }

    private static int HighestOneBit(int i) => i & (int.MinValue >>> NumberOfLeadingZeros(i));
    
    private static int NumberOfLeadingZeros(int i)
    {
        if (i <= 0)
            return i == 0 ? 32 : 0;

        var n = 31;
        
        // @formatter:off
        if (i >= 1 << 16) n -= 16; i >>>= 16;
        if (i >= 1 <<  8) n -=  8; i >>>=  8;
        if (i >= 1 <<  4) n -=  4; i >>>=  4;
        if (i >= 1 <<  2) n -=  2; i >>>=  2;
        // @formatter:on

        return n - (i >>> 1);
    }
    
}