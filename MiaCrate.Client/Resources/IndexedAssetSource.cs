using System.Text.Json;
using System.Text.Json.Nodes;
using MiaCrate.IO;
using MiaCrate.Resources;

namespace MiaCrate.Client.Resources;

public class IndexedAssetSource
{
    public static IFileSystem CreateIndexFs(string assetDir, string assetIndex)
    {
        var objects = Path.Combine(assetDir, "objects");
        var indexFile = Path.Combine(assetDir, "indexes", $"{assetIndex}.json");
        var fs = new LinkFileSystem();
        
        using var stream = File.Open(indexFile, FileMode.Open, FileAccess.Read);
        var payload = JsonSerializer.Deserialize<JsonNode>(stream)!;
        var list = payload["objects"]?.AsObject();
        
        if (list != null)
        {
            foreach (var (path, node) in list)
            {
                var hash = node!["hash"]!.GetValue<string>();
                var dir = hash[..2];
                var dest = Path.Combine(objects, dir, hash);
                fs.Register(path, dest);
            }    
        }

        return fs.Freeze();
    }
}