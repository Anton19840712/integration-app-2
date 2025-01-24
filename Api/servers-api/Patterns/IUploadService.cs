using System.Text.Json;

namespace servers_api.Patterns
{
	    /// <summary>
	    /// Сервис, который подгружает данные конфигурации в данную систему.
	    /// </summary>
	    public interface IUploadService
	    {
	        Task<string> ConfigureAsync(JsonElement jsonBody, CancellationToken stoppingToke);
	    }
}
