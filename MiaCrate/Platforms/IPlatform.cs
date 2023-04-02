using MiaCrate.Platforms.Defaults;

namespace MiaCrate.Platforms;

public interface IPlatform
{
    public IWebSocket CreateClient(Uri uri);

    public static IPlatform Default { get; } = new DefaultPlatformImpl();
}