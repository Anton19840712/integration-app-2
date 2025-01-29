using servers_api.factory.abstractions;
using servers_api.models;
using servers_api.Models;

namespace servers_api.Services.Connectors
{
	/// <summary>
	/// Сервис для настройки подключения через указанный протокол.
	/// </summary>
	public class SenderService : ISenderService
	{
		private readonly ILogger<SenderService> _logger;
		private readonly ProtocolManager _protocolManager;

		public SenderService(
			ILogger<SenderService> logger,
			ProtocolManager protocolManager)
		{
			_logger = logger;
			_protocolManager = protocolManager;
		}

		// Переименованный метод
		public async Task<ResponceIntegration> UpAsync(CombinedModel parsedModel, CancellationToken stoppingToken)
		{
			_logger.LogInformation(
				"Запуск UpAsync с протоколом: {Protocol}, роль: Сервер - {IsServer}, Клиент - {IsClient}",
				parsedModel.Protocol, parsedModel.DataOptions.IsServer, parsedModel.DataOptions.IsClient);

			var parsedModelProtocol = parsedModel.Protocol.ToUpper();
			var isServer = parsedModel.DataOptions.IsServer;
			var isClient = parsedModel.DataOptions.IsClient;

			if (!isServer && !isClient)
			{
				_logger.LogWarning("Не указана роль: необходимо указать, является ли процесс клиентом или сервером.");
				return new ResponceIntegration { Message = "Не указана роль (клиент или сервер)", Result = false };
			}

			try
			{
				if (isServer)
				{
					_logger.LogInformation("Настройка сервера с протоколом {Protocol}", parsedModelProtocol);

					var responceIntegration = await _protocolManager.ConfigureAsync(
						parsedModelProtocol,
						isServer: true,
						address: null,
						host: parsedModel.DataOptions.ServerDetails.Host,
						port: parsedModel.DataOptions.ServerDetails.Port);

					var result = responceIntegration;
					return result;
				}
				else if (isClient)
				{
					_logger.LogInformation(
						"Настройка клиента для подключения к серверу {ServerIp}:{ServerPort} с протоколом {Protocol}",
						parsedModel.DataOptions.ServerDetails.Host,
						parsedModel.DataOptions.ServerDetails.Port,
						parsedModelProtocol);

					var responceIntegration = await _protocolManager.ConfigureAsync(
						parsedModelProtocol,
						isServer: false,
						address: null,
						host: parsedModel.DataOptions.ServerDetails.Host,
						port: parsedModel.DataOptions.ServerDetails.Port);

					var result = responceIntegration;
					return result;
				}

				return new ResponceIntegration
				{
					Message = "Настройка протокола не была завершена успешно",
					Result = false
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при настройке протокола {Protocol}", parsedModelProtocol);
				return new ResponceIntegration
				{
					Message = $"Ошибка при настройке протокола: {ex.Message}",
					Result = false
				};
			}
		}
	}
}
