namespace MiaCrate;

public interface IProfileCollector : IProfilerFiller
{
    public IProfileResults Results { get; }
}