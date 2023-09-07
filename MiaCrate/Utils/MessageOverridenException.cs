namespace MiaCrate;

internal class MessageOverridenException : Exception
{
    public MessageOverridenException(string message, Exception source) : base(message, source) {}
}