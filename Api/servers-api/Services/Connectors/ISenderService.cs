using servers_api.models.internallayerusage;
using servers_api.models.responce;

namespace servers_api.Services.Connectors
{
	/// <summary>
	/// Интерфейс главного сервиса для настройки подключения согласно указанного протокола.
	/// </summary>
	public interface ISenderService
	{
		Task<ResponceIntegration> UpAsync(
			CombinedModel parsedModel,
			CancellationToken stoppingToken);
	}
}