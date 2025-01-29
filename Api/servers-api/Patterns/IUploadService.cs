using System.Text.Json;
using servers_api.models;

namespace servers_api.Patterns
{
	/// <summary>
	/// Сервис, который подгружает данные конфигурации в данную систему.
	/// </summary>
	public interface IUploadService
	{
		Task<List<ResponceIntegration>> ConfigureAsync(JsonElement jsonBody, CancellationToken stoppingToke);
	}
}
