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

    public record ProcessorCache(byte Level, byte Associativity, short LineSize, int CacheSize, ProcessorCache.CacheType Type)
    {
        public ProcessorCache(int level, int associativity, int lineSize, long cacheSize, CacheType type) 
            : this((byte) level, (byte) associativity, (short) lineSize, (int) cacheSize, type) {}
        
        public enum CacheType
        {
            Unified,
            Instruction,
            Data,
            Trace
        }
    }
}