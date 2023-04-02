using MiaCrate.Core;
using MiaCrate.Platforms;

namespace MiaCrate;

public static class MochiCore
{
    private static bool _bootstrapped;
    
    public static IPlatform Platform { get; private set; }

    public static void Bootstrap(IPlatform platform)
    {
        if (_bootstrapped) return;
        _bootstrapped = true;
        Platform = platform;

        if (!Registry.Root.KeySet.Any())
        {
            throw new Exception("Unable to load registries");
        }
    }
}