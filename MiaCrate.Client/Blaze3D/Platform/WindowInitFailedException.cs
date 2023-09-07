namespace MiaCrate.Client.Platform;

public class WindowInitFailedException : SilentInitException
{
    public WindowInitFailedException(string message) : base(message) {}
}