namespace MiaCrate;

public static class LogUtils
{
    public static object Defer<T>(Func<T> result) => new DeferredToString<T>(result);

    private class DeferredToString<T>
    {
        private readonly Func<T> _result;

        public DeferredToString(Func<T> result)
        {
            _result = result;
        }

        public override string ToString() => _result()!.ToString()!;
    }
}