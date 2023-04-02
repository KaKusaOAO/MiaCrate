using Mochi.Platforms.Defaults;

namespace Mochi.Platforms;

public interface IPlatform
{
    public IWebSocket CreateClient(Uri uri);

    public static IPlatform Default { get; } = new DefaultPlatformImpl();
}