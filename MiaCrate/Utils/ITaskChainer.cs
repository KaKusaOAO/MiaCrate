using MiaCrate.Common;
using Mochi.Utils;

namespace MiaCrate;

public interface ITaskChainer
{
    public static ITaskChainer CreateImmediate(IExecutor executor)
    {
        return new Immediate(t =>
        {
            t.SubmitAsync(executor).ExceptionallyAsync(ex =>
            {
                Logger.Warn("Task failed");
                Logger.Warn(ex);
            });
        });
    }
    
    public void Append(IDelayedTask task);

    private class Immediate : ITaskChainer
    {
        private readonly Action<IDelayedTask> _action;

        public Immediate(Action<IDelayedTask> action)
        {
            _action = action;
        }

        public void Append(IDelayedTask task) => _action(task);
    }
}

public interface IDelayedTask
{
    public Task SubmitAsync(IExecutor executor);
}