using System.Text.Json.Serialization;

namespace servers_api.Models;

    public class DataOptions
    {
        [JsonPropertyName("client")]
        public bool IsClient { get; set; }

        [JsonPropertyName("server")]
        public bool IsServer { get; set; }

        [JsonPropertyName("serverDetails")]
        public ConnectionDetails ServerDetails { get; set; }
    }
