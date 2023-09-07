namespace MiaCrate.Client;

public class SilentInitException : Exception
{
    public SilentInitException(string message) : base(message) {}
    public SilentInitException(string message, Exception inner) : base(message, inner) {}
}