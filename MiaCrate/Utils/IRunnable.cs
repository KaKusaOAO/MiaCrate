namespace MiaCrate;

public interface IRunnable
{
    public void Run();

    public static IRunnable Create(Action action) => new Direct(action);

    private class Direct : IRunnable
    {
        private readonly Action _action;
        
        public Direct(Action action)
        {
            _action = action;
        }

        public void Run() => _action();
    }
}