using System.Text.Json;
using servers_api.main.facades;
using servers_api.models.entities;
using servers_api.models.response;

namespace servers_api.main.services;

/// <summary>
/// Общий менеджер-сервис, занимающийся процессингом настройки
/// всей инфраструктуры динамического шлюза под отдельную организацию.
/// </summary>
public class TeachIntegrationService(
	QueuesRepository queuesRepository,
	IIntegrationFacade integrationFacade,
	ILogger<TeachIntegrationService> logger) : ITeachIntegrationService
{
	public async Task<List<ResponseIntegration>> TeachAsync(
		JsonElement jsonBody,
		CancellationToken stoppingToken)
	{
		logger.LogInformation("Начало обработки TeachAsync");

		try
		{
			//1
			logger.LogInformation("Выполняется ParseJsonAsync.");
			var parsedModel = await integrationFacade.ParseJsonAsync(jsonBody, true, stoppingToken);

			//2
			logger.LogInformation("Выполняется сохранение в базу очередей.");
			var modelQueueSave = new QueuesEntity() { 
				InQueueName = parsedModel.InQueueName,
				OutQueueName = parsedModel.OutQueueName
			};


			var existingModel = (await queuesRepository.FindAsync(x =>
				x.InQueueName == parsedModel.InQueueName &&
				x.OutQueueName == parsedModel.OutQueueName)).FirstOrDefault();

			if (existingModel != null)
			{
				parsedModel.Id = existingModel.Id; // Сохраняем ID:
				await queuesRepository.UpdateAsync(existingModel.Id, modelQueueSave);
			}
			else
			{
				// Если модели нет — вставляем новую:
				await queuesRepository.InsertAsync(modelQueueSave);
			}
			await queuesRepository.InsertAsync(modelQueueSave);

			//3
			logger.LogInformation("Выполняется ExecuteTeachAsync.");
			var apiStatus = await integrationFacade.TeachBpmAsync(
				parsedModel,
				stoppingToken);

			//4
			logger.LogInformation("Запускаем слушателя в фоне для очереди: {Queue}.", parsedModel.OutQueueName);

			var elements = await queuesRepository.GetAllAsync();

			foreach (var element in elements)
			{
				// TODO: use parallel foreach:
				await integrationFacade.StartListeningAsync(
				element.OutQueueName,
				stoppingToken);
			}

			return [
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
