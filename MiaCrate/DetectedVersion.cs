using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate;

public class DetectedVersion : IWorldVersion
{
    public static readonly IWorldVersion BuiltIn = new DetectedVersion();
    
    public DataVersion DataVersion { get; }

    public string Id { get; }

    public string Name { get; }

    public int ProtocolVersion { get; }

    public DateTimeOffset BuildTime { get; }

    public bool IsStable { get; }

    private readonly int _resourcePackVersion;
    private readonly int _dataPackVersion;

    private DetectedVersion()
    {
        Id = Uuid.NewUuid().ToString().Replace("-", "");
        Name = SharedConstants.VersionString;
        IsStable = !SharedConstants.IsSnapshot;
        DataVersion = new DataVersion(SharedConstants.WorldVersion, SharedConstants.Series);
        ProtocolVersion = SharedConstants.ProtocolVersion;
        _resourcePackVersion = SharedConstants.ResourcePackFormat;
        _dataPackVersion = SharedConstants.DataPackFormat;
        BuildTime = DateTimeOffset.Now;
    }

    private DetectedVersion(JsonObject obj)
    {
        Id = obj["id"]!.GetValue<string>();
        Name = obj["name"]!.GetValue<string>();
        IsStable = obj["stable"]!.GetValue<bool>();
        DataVersion = new DataVersion(
            obj["world_version"]!.GetValue<int>(),
            obj["series_id"]!.GetValue<string>()
        );
        ProtocolVersion = obj["protocol_version"]!.GetValue<int>();

        var packVersion = obj["pack_version"]!.AsObject();
        _resourcePackVersion = packVersion["resource"]!.GetValue<int>();
        _dataPackVersion = packVersion["data"]!.GetValue<int>();

        BuildTime = DateTimeOffset.Parse(obj["build_time"]!.GetValue<string>());
    }

    public int GetPackVersion(PackType type) => 
        type == PackType.ServerData ? _dataPackVersion : _resourcePackVersion;

    public static IWorldVersion TryDetectVersion()
    {
        var archive = ResourceAssembly.GameArchive;
        var entry = archive.GetEntry("version.json");
        if (entry == null)
        {
            Logger.Warn("Cannot read version.json! Using builtin version...");
            return BuiltIn;
        }

        using var versionStream = entry.Open();
        var obj = JsonNode.Parse(versionStream)!.AsObject();
        return new DetectedVersion(obj);
    }
}