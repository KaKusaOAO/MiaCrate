using System.Text.Json.Serialization;
using MiaCrate.Json;
using Mochi.Texts;

namespace MiaCrate.Net.Packets.Status;

public class ServerStatus
{
    [JsonPropertyName("version")]
    public VersionData Version { get; set; }
    
    [JsonPropertyName("players")]
    public PlayersData Players { get; set; }
    
    [JsonConverter(typeof(ComponentConverter))]
    [JsonPropertyName("description")]
    public IComponent Description { get; set; }
    
    [JsonPropertyName("enforcesSecureChat")]
    public bool EnforcesSecureChat { get; set; }
    
    [JsonPropertyName("previewsChat")]
    public bool PreviewsChat { get; set; }

    public class VersionData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    
        [JsonPropertyName("protocol")]
        public int Protocol { get; set; }
    }

    public class PlayersData
    {
        [JsonPropertyName("max")]
        public int Max { get; set; }
    
        [JsonPropertyName("online")]
        public int Online { get; set; }
    
        [JsonPropertyName("sample")]
        public List<PlayerSample> Sample { get; set; }
    }

    public class PlayerSample
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}