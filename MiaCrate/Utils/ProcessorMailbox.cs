using MiaCrate.Common;

namespace MiaCrate;

public class ProcessorMailbox<T> : IProfilerMeasured, IProcessorHandle<T>, IDisposable, IRunnable
{
    public void Tell(T message)
    {
        throw new NotImplementedException();
    }

    public void Run()
    {
        throw new NotImplementedException();
    }
}