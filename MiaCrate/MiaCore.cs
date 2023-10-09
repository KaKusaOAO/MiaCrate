using MiaCrate.Core;
using MiaCrate.Platforms;
using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate;

public static class MiaCore
{
    public const string ProductName = "MiaCrate";
    
    private static IPlatform? _platform;
    private static bool _bootstrapped;
    
    public static IPlatform Platform => _platform ?? throw new Exception("Not bootstrapped");
    
    public static void Bootstrap(IPlatform platform)
    {
        if (_bootstrapped) return;
        _bootstrapped = true;
        _platform = platform;

        Component.StyleProvider = new MiaStyleProvider();

        if (!BuiltinRegistries.Root.KeySet.Any())
        {
            throw new Exception("Unable to load registries");
        }
        
        BuiltinRegistries.Bootstrap();
        MiaContentTypes.Bootstrap();
    }
}