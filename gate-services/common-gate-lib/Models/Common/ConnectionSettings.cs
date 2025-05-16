using System.Text.Json.Serialization;

namespace CommonGateLib.Models.Common
{
	public record class ConnectionSettings
	{
		[JsonPropertyName("clientSettings")]
		public ClientSettings ClientConnectionSettings { get; set; }

		[JsonPropertyName("serverSettings")]
		public ServerSettings ServerConnectionSettings { get; set; }
	}
}
