using CommandLine;

namespace MiaCrate.Client;

public class BootstrapOptions
{
    [Option("demo")]
    public bool IsDemo { get; set; }
    
    [Option("disableMultiplayer")]
    public bool DisableMultiplayer { get; set; }
    
    [Option("disableChat")]
    public bool DisableChat { get; set; }
    
    [Option("fullscreen")]
    public bool IsFullscreen { get; set; }
    
    [Option("checkGlErrors")]
    public bool CheckGlErrors { get; set; }
    
    [Option("quickPlayPath")]
    public string? QuickPlayPath { get; set; }

    [Option("quickPlaySingleplayer")] 
    public string? QuickPlaySinglePlayer { get; set; }

    [Option("quickPlayMultiplayer")]
    public string? QuickPlayMultiPlayer { get; set; }
    
    [Option("quickPlayRealms")]
    public string? QuickPlayRealms { get; set; }

    [Option("gameDir", Default = ".")] 
    public string GameDirectory { get; set; } = ".";
    
    [Option("assetsDir")]
    public string? AssetsDirectory { get; set; }
    
    [Option("resourcePackDir")]
    public string? ResourcePackDirectory { get; set; }
    
    [Option("proxyHost")]
    public string? ProxyHost { get; set; }
    
    [Option("proxyPort", Default = 8080)]
    public int ProxyPort { get; set; }
    
    [Option("proxyUser")]
    public string? ProxyUser { get; set; }
    
    [Option("proxyPass")]
    public string? ProxyPass { get; set; }
    
    [Option("username")]
    public string? Username { get; set; }
    
    [Option("uuid")]
    public string? Uuid { get; set; }

    [Option("xuid", Default = "")] 
    public string Xuid { get; set; } = "";

    [Option("clientId", Default = "")]
    public string ClientId { get; set; } = "";

    [Option("accessToken", Required = true)]
    public string AccessToken { get; set; } = null!;

    [Option("version", Required = true)] 
    public string Version { get; set; } = null!;
    
    [Option("width", Default = 854)]
    public int WindowWidth { get; set; }
    
    [Option("height", Default = 480)]
    public int WindowHeight { get; set; }
    
    [Option("fullscreenWidth")]
    public int? FullscreenWidth { get; set; }
    
    [Option("fullscreenHeight")]
    public int? FullscreenHeight { get; set; }
    
    [Option("userProperties", Default = "{}")]
    public string UserProperties { get; set; } = "{}";
    
    [Option("profileProperties", Default = "{}")]
    public string ProfileProperties { get; set; } = "{}";
    
    [Option("assetIndex")]
    public string? AssetIndex { get; set; }

    [Option("userType", Default = "legacy")]
    public string UserType { get; set; } = "legacy";

    [Option("versionType", Default = "release")]
    public string VersionType { get; set; } = "release";
}