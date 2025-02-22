using System.Text.Json;
using servers_api.main.facades;
using servers_api.models.response;

namespace servers_api.main.services;

/// <summary>
/// Общий менеджер-сервис, занимающийся процессингом настройки
/// всей инфраструктуры динамического шлюза под отдельную организацию.
/// </summary>
public class TeachIntegrationService(
	IIntegrationFacade integrationFacade,
	ILogger<TeachIntegrationService> logger) : ITeachIntegrationService
{
	public async Task<List<ResponseIntegration>> TeachAsync(JsonElement jsonBody, CancellationToken stoppingToken)
	{
		logger.LogInformation("Начало обработки TeachAsync");

		try
		{
			//1
			logger.LogInformation("Выполняется ParseJsonAsync.");
			var parsedModel = await integrationFacade.ParseJsonAsync(jsonBody, true, stoppingToken);

			//2
			logger.LogInformation("Выполняется CreateQueuesAsync.");
			var resultOfCreation = await integrationFacade.CreateQueuesAsync(
				parsedModel.InQueueName,
				parsedModel.OutQueueName,
				stoppingToken);

			//3
			logger.LogInformation("Выполняется ExecuteTeachAsync.");
			var apiStatus = await integrationFacade.TeachBpmAsync(
				parsedModel,
				stoppingToken);

			//4
			logger.LogInformation("Запускаем слушателя в фоне для очереди: {Queue}", parsedModel.OutQueueName);
			_ = Task.Run(() => integrationFacade.StartListeningAsync(
				parsedModel.OutQueueName,
				stoppingToken),
				stoppingToken);

			return [
				resultOfCreation,
				apiStatus,
				new ResponseIntegration {
					Message = $"Cлушатель очeреди {parsedModel.OutQueueName} запустился.",
					Result = true
				}
			];
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка в процессе TeachAsync");
			throw;
		}
	}
}
