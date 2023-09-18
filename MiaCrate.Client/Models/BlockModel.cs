using System.Text.Json;

namespace MiaCrate.Client.Models;

public class BlockModel : IUnbakedModel
{
    public static BlockModel FromStream(Stream stream)
    {
        var json = JsonSerializer.Deserialize<JsonBlockModel>(stream);
        return new BlockModel();
    }
}