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
            const string message = "An exception occurred while reloading.";
            
            var ex = Task.Exception!;
            var list = ex.InnerExceptions;
            if (list.Count == 1)
                throw new Exception(message, list.First());

            throw new Exception(message, ex);
        }
    }
}