namespace MiaCrate.Client.Utils;

public interface IProcessorHandle<T> : IDisposable
{
    public void Tell(T message);

    public new void Dispose()
    {
        
    }

    void IDisposable.Dispose() => Dispose();
}