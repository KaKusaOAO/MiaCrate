namespace MiaCrate.Client.Oshi;

public interface ICentralProcessor
{
    public ProcessorIdentifier ProcessorIdentifier { get; }
    public long MaxFrequency { get; }
    public long[] CurrentFrequency { get; }
    public List<LogicalProcessor> LogicalProcessors { get; }
    public List<PhysicalProcessor> PhysicalProcessors { get; }
    public double GetSystemCpuLoadBetweenTicks(long[] result);
    public long[] GetSystemCpuLoadTicks();
    public int LogicalProcessorCount { get; }
    public int PhysicalProcessorCount { get; }
    public double[] GetSystemLoadAverage();
}