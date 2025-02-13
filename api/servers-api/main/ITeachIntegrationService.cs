using System.Text.Json;
using servers_api.models.response;

namespace servers_api.main;

/// <summary>
/// Сервис, который подгружает данные конфигурации в эту систему.
/// </summary>
public interface ITeachIntegrationService
{
	Task<List<ResponseIntegration>> TeachAsync(
		JsonElement jsonBody,
		CancellationToken stoppingToke);
}
