// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Text.Json;
using CommandLine;
using MiaCrate;
using MiaCrate.Auth;
using MiaCrate.Client;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using MiaCrate.Client.Test;
using Mochi.Utils;
using IPlatform = MiaCrate.Platforms.IPlatform;

Logger.Logged += Logger.LogToEmulatedTerminalAsync;
Logger.RunThreaded();
;
var result = new Parser(settings =>
    {
        settings.AutoHelp = false;
    })
    .ParseArguments<BootstrapOptions>(args);

if (result.Tag == ParserResultType.NotParsed)
{
    Logger.Error("Booting up MiaCrate requires access token and version.");
    Logger.Error("Exiting...");
    return;
}

var options = result.Value;
options.Username ??= "Player" + Util.GetMillis() % 1000;

var proxy = new WebProxy();
if (options.ProxyHost != null)
{
    var builder = new UriBuilder
    {
        Host = options.ProxyHost,
        Port = options.ProxyPort
    };

    Logger.Info(builder.Uri);
    proxy.Address = builder.Uri;

    if (!string.IsNullOrEmpty(options.ProxyUser) && !string.IsNullOrEmpty(options.ProxyPass))
    {
        proxy.Credentials = new CredentialCache
        {
            { builder.Uri, "Basic", new NetworkCredential(options.ProxyUser, options.ProxyPass) }
        };
    }
}

var userProps = JsonSerializer.Deserialize<PropertyMap>(options.UserProperties)!;
var profileProps = JsonSerializer.Deserialize<PropertyMap>(options.ProfileProperties)!;
var gameDir = options.GameDirectory;
var assetsDir = options.AssetsDirectory ?? Path.Combine(gameDir, "assets/");
var resourcePackDir = options.ResourcePackDirectory ?? Path.Combine(gameDir, "resourcepacks/");

CrashReport.Preload();
MiaCore.Bootstrap(IPlatform.Default);

var type = UserType.ByName(options.UserType);
if (type == null)
{
    type = UserType.Legacy;
    Logger.Warn($"Unrecognized user type: {options.UserType}");    
}

var uuid = options.Uuid ?? UuidHelper.CreateOfflinePlayerUuid(options.Username).ToString();
var user = new User(options.Username, uuid, options.AccessToken, options.Xuid, options.ClientId, type);
var config = new GameConfig(
    new UserData(user, userProps, profileProps, proxy),
    new DisplayData(options.WindowWidth, options.WindowHeight, options.FullscreenWidth, options.FullscreenHeight, options.IsFullscreen),
    new FolderData(gameDir, resourcePackDir, assetsDir, options.AssetIndex),
    new GameData(options.IsDemo, options.Version, options.VersionType, options.DisableMultiplayer, options.DisableChat),
    new QuickPlayData(options.QuickPlayPath, options.QuickPlaySinglePlayer, options.QuickPlayMultiPlayer, options.QuickPlayRealms)
);

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    var game = Game.Instance;
    if (game == null) return;
    
    Logger.Info("Stopping singleplayer server");
};

Game game;
try
{
    Thread.CurrentThread.Name = "Render thread";
    RenderSystem.InitRenderThread();
    RenderSystem.BeginInitialization();
    game = new Game(config);
    Logger.Info(GLX.CpuInfo);
}
catch (SilentInitException ex)
{
    Logger.Warn($"Failed to create window");
    Logger.Warn(ex);
    return;
}
catch (Exception ex)
{
    var report = CrashReport.ForException(ex, "Initializing game");
    Game.Crash(report);
    return;
}

Thread? thread;
if (game.IsRenderedOnThread)
{
    thread = new Thread(() =>
    {
        try
        {
            RenderSystem.InitGameThread(true);
            game.Run();
        }
        catch (Exception ex)
        {
            Logger.Error("Exception in client thread");
            Logger.Error(ex);
        }
    });
    thread.Start();

    while (game.IsRunning)
    {
        
    }
}
else
{
    thread = null;

    try
    {
        RenderSystem.InitGameThread(false);
        game.Run();
    }
    catch (Exception ex)
    {
        Logger.Error("Unhandled game exception");
        Logger.Error(ex);
    }
}

try
{
    game.Stop();
    thread?.Join();
}
catch (ThreadInterruptedException ex)
{
    Logger.Error("Exception during client thread shutdown");
    Logger.Error(ex);
}
finally
{
    game.Destroy();
}