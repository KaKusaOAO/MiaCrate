using System.Collections.Immutable;
using System.Net;
using MiaCrate.Auth;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Resources;
using MiaCrate.IO;

namespace MiaCrate.Client;

public record GameConfig(
    UserData User, 
    DisplayData Display, 
    FolderData Location, 
    GameData Game, 
    QuickPlayData QuickPlay);

public record FolderData(
    string GameDirectory,
    string ResourcePackDirectory,
    string AssetDirectory,
    string? AssetIndex)
{
    public IFileSystem ExternalAssetSource =>
        AssetIndex == null 
            ? new RelativeRootedDefaultFileSystem(Path.GetFullPath(AssetDirectory)) 
            : IndexedAssetSource.CreateIndexFs(Path.GetFullPath(AssetDirectory), AssetIndex);
}

public record GameData(
    bool IsDemo, 
    string LaunchVersion, 
    string VersionType, 
    bool IsMultiplayerDisabled, 
    bool IsChatDisabled);
public record QuickPlayData(
    string? Path, 
    string? Singleplayer, 
    string? Multiplayer, 
    string? Realms);
public record UserData(
    User User, 
    PropertyMap UserProperties, 
    PropertyMap ProfileProperties,
    IWebProxy Proxy);

public class User
{
    public string Name { get; }
    public string Uuid { get; }
    public string AccessToken { get; }
    public string? Xuid { get; }
    public string? ClientId { get; }
    public UserType Type { get; }
    
    public User(string name, string uuid, string accessToken, string? xuid, string? clientId, UserType type)
    {
        Name = name;
        Uuid = uuid;
        AccessToken = accessToken;
        Xuid = xuid;
        ClientId = clientId;
        Type = type;
    }

    public string SessionId => $"token:{AccessToken}:{Uuid}";
}

public class UserType
{
    private static readonly Dictionary<int, UserType> _values = new();

    public static readonly UserType Legacy = new("legacy");
    public static readonly UserType Mojang = new("mojang");
    public static readonly UserType Msa = new("msa");

    private static readonly IDictionary<string, UserType> _byName = _values.Values.ToImmutableDictionary(
        x => x.Name, x => x);

    public static UserType? ByName(string name) => _byName.TryGetValue(name, out var result) ? result : null;
    
    public string Name { get; }
    public int Ordinal { get; }
    
    private UserType(string name)
    {
        Name = name;

        var ordinal = _values.Count;
        Ordinal = ordinal;
        _values[ordinal] = this;
    }
}