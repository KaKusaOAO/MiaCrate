namespace MiaCrate.Resources;

public interface IReloadInstance
{
    public Task Task { get; }
    public float ActualProgress { get; }
    public bool IsDone => Task.IsCompleted;

    public void CheckExceptions()
    {
        if (!Task.IsCompletedSuccessfully)
        {
            throw Task.Exception!;
        }
    }
}