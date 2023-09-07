using System.Diagnostics.CodeAnalysis;
using MiaCrate.Data;

namespace MiaCrate.Client.Oshi;

public abstract class AbstractCentralProcessor : ICentralProcessor
{
    public ProcessorIdentifier ProcessorIdentifier => throw new NotImplementedException();

    public long MaxFrequency => throw new NotImplementedException();

    public long[] CurrentFrequency => throw new NotImplementedException();

    public List<LogicalProcessor> LogicalProcessors { get; }

    public List<PhysicalProcessor> PhysicalProcessors { get; }

    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected AbstractCentralProcessor()
    {
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
}