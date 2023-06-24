using MiaCrate.Core;
using MiaCrate.Platforms;
using MiaCrate.Texts;

namespace MiaCrate;

public static class MiaCore
{
    private static bool _bootstrapped;
    private static IPlatform? _platform;
    public static IPlatform Platform => _platform ?? throw new Exception("Not bootstrapped");

    public static void Bootstrap(IPlatform platform)
    {
        if (_bootstrapped) return;
        _bootstrapped = true;
        _platform = platform;

        if (!BuiltinRegistries.Root.KeySet.Any())
        {
            throw new Exception("Unable to load registries");
        }
        
        MiaContentTypes.Init();
    }
}