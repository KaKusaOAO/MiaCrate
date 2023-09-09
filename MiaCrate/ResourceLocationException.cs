namespace MiaCrate;

public class ResourceLocationException : Exception
{
    public ResourceLocationException(string message) : base(message) {}
    public ResourceLocationException(string message, Exception inner) : base(message, inner) {}
}