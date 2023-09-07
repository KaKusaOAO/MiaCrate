namespace MiaCrate;

public interface ITimeSource
{
    public long Get(TimeUnit unit);
}

public interface INanoTimeSource : ITimeSource
{
    public long GetNanos();
    
    long ITimeSource.Get(TimeUnit unit) =>
        unit.Convert(GetNanos(), TimeUnit.Nanoseconds);

    public static INanoTimeSource Create(Func<long> supplier) => new Instance(supplier);

    private class Instance : INanoTimeSource
    {
        private readonly Func<long> _supplier;

        public Instance(Func<long> supplier)
        {
            _supplier = supplier;
        }

        public long GetNanos() => _supplier();
    }
}