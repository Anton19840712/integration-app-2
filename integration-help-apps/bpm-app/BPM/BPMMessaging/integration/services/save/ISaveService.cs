using System.Text.Json;

namespace BPMMessaging.integration.Services.Save
{
	public interface ISaveService
	{
		Task<IntegrationEntity> SaveIntegrationModelAsync(JsonElement model);
	}
}