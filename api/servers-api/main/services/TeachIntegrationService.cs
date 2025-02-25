using System.Text.Json;
using servers_api.main.facades;
using servers_api.models.entities;
using servers_api.models.response;
using servers_api.repositories;

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

			//2 логика работы с коллекцией базы данных: 
			logger.LogInformation("Выполняется сохранение в базу очередей.");
			var modelQueueSave = new QueuesEntity() { 
				InQueueName = parsedModel.InQueueName,
				OutQueueName = parsedModel.OutQueueName
			};

			//если модель с такими названиями очередей существует:
			var existingModel = (await queuesRepository.FindAsync(x =>
				x.InQueueName == parsedModel.InQueueName &&
				x.OutQueueName == parsedModel.OutQueueName)).FirstOrDefault();

			if (existingModel != null)
			{
				//меняем ей Id и обновляем:
				parsedModel.Id = existingModel.Id;
				await queuesRepository.UpdateAsync(existingModel.Id, modelQueueSave);
			}
			else
			{
				// Если модели нет — вставляем эту новую:
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

			//для каждой очереди запускаем слушателя:
			//эти логику нужно таким же образом использовать, когда ты будешь слушать данные из определенных очередей:
			//когда будешь получать данные на сервер - однако, такое ощущение, что ты должен будешь заслушать только те данные, которые у тебя
			//будут относиться к tcp - соединению? Или какая разница?
			//по идее у тебя может быть создан очень умные лисенер.
			//если на стороне бпм мы можем говорить про то, что у нас будет всеядный лисенер,
			//то на стороне интеграционного динамического шлюза, наверное, это будет какой-то выборочный слушатель?

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
