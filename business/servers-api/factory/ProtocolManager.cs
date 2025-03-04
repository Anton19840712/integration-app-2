using servers_api.factory;
using servers_api.models.internallayer.instance;
using servers_api.models.response;

public class ProtocolManager : IProtocolManager
{
	private readonly IProtocolFactory _protocolFactory;
	private readonly ILogger<ProtocolManager> _logger;

	public ProtocolManager(IProtocolFactory protocolFactory, ILogger<ProtocolManager> logger)
	{
		_protocolFactory = protocolFactory;
		_logger = logger;
	}

	public async Task<ResponseIntegration> ConfigureAsync(InstanceModel instanceModel)
	{
		var factory = _protocolFactory.GetFactory(instanceModel.Protocol);

		if (instanceModel is ClientInstanceModel clientModel)
		{
			return await ConfigureClientAsync(factory, clientModel);
		}
		else if (instanceModel is ServerInstanceModel serverModel)
		{
			return await ConfigureServerAsync(factory, serverModel);
		}

		return new ResponseIntegration
		{
			Message = "Неизвестный тип инстанса",
			Result = false
		};
	}

	private async Task<ResponseIntegration> ConfigureClientAsync(UpInstanceByProtocolFactory factory, ClientInstanceModel clientModel)
	{
		var client = factory.CreateClient();
		var serverHost = clientModel.ServerHostPort.Host;
		var serverPort = clientModel.ServerHostPort.Port ?? 80;

		_logger.LogInformation("Настройка клиента {Protocol} для подключения к серверу {Host}:{Port}",
			clientModel.Protocol, serverHost, serverPort);

		using var cts = new CancellationTokenSource(clientModel.ClientConnectionSettings.ConnectionTimeoutMs);
		return await client.ConnectToServerAsync(clientModel, serverHost, serverPort, cts.Token);
	}

	private async Task<ResponseIntegration> ConfigureServerAsync(UpInstanceByProtocolFactory factory, ServerInstanceModel serverModel)
	{
		var server = factory.CreateServer();
		_logger.LogInformation("Запуск сервера {Protocol} на {Host}:{Port}",
			serverModel.Protocol, serverModel.Host, serverModel.Port);

		using var cts = new CancellationTokenSource(serverModel.ServerConnectionSettings.BusIdleTimeoutMs);
		return await server.UpServerAsync(serverModel, cts.Token);
	}
}
