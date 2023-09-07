namespace MiaCrate;

public interface IExecutor
{
    public void Execute(IRunnable runnable);
    public void Execute(Action action) => Execute(IRunnable.Create(action));

    public static IExecutor Create(Action<IRunnable> action) => new Instance(action);
    
    private class Instance : IExecutor
    {
        private readonly Action<IRunnable> _action;

        public Instance(Action<IRunnable> action)
        {
            _action = action;
        }

        public void Execute(IRunnable runnable)
        {
            _action(runnable);
        }
    }
}