using AutoMapper;
using servers_api.factory.abstractions;
using servers_api.models.internallayer.common;
using servers_api.models.internallayer.instance;
using servers_api.models.response;
using servers_api.Services.Connectors;

namespace servers_api.services.connectors
{
	public class SenderService(
		ILogger<SenderService> logger,
		IProtocolManager protocolManager,
		IMapper mapper) : ISenderService
	{
		public async Task<ResponseIntegration> UpAsync(
			CombinedModel parsedModel,
			CancellationToken stoppingToken)
		{
			logger.LogInformation(
				"Запуск UpAsync метода с протоколом: {Protocol}, роль: Сервер - {IsServer}, Клиент - {IsClient}",
				parsedModel.Protocol,
				parsedModel.DataOptions.IsServer,
				parsedModel.DataOptions.IsClient);

			// Используем AutoMapper для маппинга
			InstanceModel instanceModel = parsedModel.DataOptions.IsClient
				? mapper.Map<ClientInstanceModel>(parsedModel)
				: mapper.Map<ServerInstanceModel>(parsedModel);

			if (instanceModel is ClientInstanceModel clientModel)
			{
				logger.LogInformation("Настройка клиента с хостом {Host} и портом {Port}",
					clientModel.ClientHost,
					clientModel.ClientPort);

				// Передаем всю модель в метод ConfigureNodeAsync
				var responseIntegration = await protocolManager.ConfigureAsync(
					clientModel);

				return responseIntegration;
			}
			else if (instanceModel is ServerInstanceModel serverModel)
			{
				logger.LogInformation("Настройка сервера с хостом {Host} и портом {Port}",
					serverModel.Host,
					serverModel.Port);

				// Передаем всю модель в метод ConfigureNodeAsync
				var responseIntegration = await protocolManager.ConfigureAsync(
					serverModel);

				return responseIntegration;
			}

			return new ResponseIntegration
			{
				Message = "Настройка протокола не была завершена успешно.",
				Result = false
			};
		}
	}
}
