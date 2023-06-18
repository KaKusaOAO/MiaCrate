using MiaCrate.Core;
using MiaCrate.Platforms;

namespace MiaCrate;

public static class MiaCore
{
    private static bool _bootstrapped;
    
    public static IPlatform Platform { get; private set; }

    public static void Bootstrap(IPlatform platform)
    {
        if (_bootstrapped) return;
        _bootstrapped = true;
        Platform = platform;

        if (!BuiltinRegistries.Root.KeySet.Any())
        {
            throw new Exception("Unable to load registries");
        }
    }
}