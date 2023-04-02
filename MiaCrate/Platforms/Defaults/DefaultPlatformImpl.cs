namespace MiaCrate.Platforms.Defaults;

public class DefaultPlatformImpl : IPlatform
{
    public IWebSocket CreateClient(Uri uri) => WebSocketImpl.CreateClient(uri);
}