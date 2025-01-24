using System.Text;
using Newtonsoft.Json;
using servers_api.Models;
using ILogger = Serilog.ILogger;

namespace servers_api.Services.InternalSystems
{
	public class TeachService : ITeachService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger _logger;

		public TeachService(IHttpClientFactory httpClientFactory, ILogger logger)
		{
			_httpClientFactory = httpClientFactory;
			_logger = logger;
		}

		public async Task<ResponceIntegration> TeachBPMNAsync(CombinedModel parsedModel, CancellationToken token)
		{
			_logger.Information("Начало обработки TeachBPMNAsync");

			var modelForCardSystem = new InMessage
			{
				InternalModel = parsedModel.InternalModel,
				QueuesNames = new QueuesNames
				{
					InQueueName = parsedModel.InQueueName,
					OutQueueName = parsedModel.OutQueueName
				}
			};

			var client = _httpClientFactory.CreateClient();

			try
			{
				// Сериализация объекта в JSON
				var jsonContent = new StringContent(
					JsonConvert.SerializeObject(modelForCardSystem),
					Encoding.UTF8,
					"application/json"
				);

				_logger.Information("Отправка POST-запроса на https://localhost:7054/Integration/save");

				// Отправляем POST-запрос с телом
				// Эти адреса необходимо сделать динамическими
				var response = await client.PostAsync("https://localhost:7054/Integration/save", jsonContent, token);

				if (response.IsSuccessStatusCode)
				{
					_logger.Information("Соединение успешно установлено с API, статус-код: {StatusCode}", response.StatusCode);
					return new ResponceIntegration
					{
						Message = "API доступен, соединение успешно установлено.",
						Result = true
					};
				}
				else
				{
					_logger.Warning("API недоступен, статус-код: {StatusCode}", response.StatusCode);
					return new ResponceIntegration
					{
						Message = $"API недоступен. Статус-код: {(int)response.StatusCode}",
						Result = false
					};
				}
			}
			catch (HttpRequestException ex)
			{
				_logger.Error(ex, "Ошибка при обращении к API");
				return new ResponceIntegration
				{
					Message = $"Ошибка при обращении к API: {ex.Message}",
					Result = false
				};
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Неожиданная ошибка при проверке статуса API");
				return new ResponceIntegration
				{
					Message = $"Неожиданная ошибка при проверке статуса API: {ex.Message}",
					Result = false
				};
			}
		}
	}
}
