using System.Text.Json;

namespace BPMMessaging.integration.services.parsing
{
	public interface IJsonParsingService
	{
		IntegrationEntity ParseJson(JsonElement jsonBody);
	}
}