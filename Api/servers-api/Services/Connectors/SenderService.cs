using servers_api.Factory.Abstractions;
using servers_api.Models;
using ILogger = Serilog.ILogger;

namespace servers_api.Services.Connectors;

    /// <summary>
    /// Надо понять, что делает данные сервис и когда он подключается в работу.
    /// </summary>
    public class SenderService : ISenderService
    {
        private readonly ILogger _logger;

        public SenderService(ILogger logger)
        {
            _logger = logger;
        }

        //TODO переименуй метод в UP
        public async Task<ResponceIntegration> RunServerByProtocolTypeAsync(CombinedModel parsedModel, CancellationToken stoppingToken)
        {
            _logger.Information(
                "Запуск RunServerByProtocolTypeAsync с протоколом: {Protocol} и параметрами роли: Сервер - {IsServer}, Клиент - {IsClient}",
                parsedModel.Protocol, parsedModel.DataOptions.IsServer, parsedModel.DataOptions.IsClient);

            var parsedModelProtocol = parsedModel.Protocol.ToUpper();
            var manager = new ProtocolManager();
            var isServer = parsedModel.DataOptions.IsServer;
            var isClient = parsedModel.DataOptions.IsClient;

		// Тут мы смотрим сначала роль нода (клиент мы будем запускать или сервер)
		// А уже внутри manager.Configure мы определяемся с созданием определенного типа соединения по протоколу
            // То есть тут это такая обертка, которая делит логику на клиент или сервер
		if (isServer)
            {
                if (parsedModelProtocol == "TCP" || parsedModelProtocol == "UDP")
                {
                    _logger.Information("Настройка сервера с протоколом {Protocol}", parsedModelProtocol);
                    manager.Configure(
                        parsedModelProtocol,
                        isServer: true,
                        null,
                        parsedModel.DataOptions.ServerDetails.Host,
                        parsedModel.DataOptions.ServerDetails.Port);
                }
                else
                {
                    _logger.Warning("Неизвестный протокол {Protocol} для сервера", parsedModelProtocol);
                    return new ResponceIntegration { Message = "Неизвестный протокол для сервера", Result = false };
                }
            }
            else if (isClient)
            {
                var serverIp = parsedModel.DataOptions.ServerDetails.Host;
                var serverPort = parsedModel.DataOptions.ServerDetails.Port;

                if (parsedModelProtocol == "TCP" || parsedModelProtocol == "UDP")
                {
                    _logger.Information("Настройка клиента для подключения к серверу {ServerIp}:{ServerPort} с протоколом {Protocol}", serverIp, serverPort, parsedModelProtocol);
                    manager.Configure(parsedModelProtocol, isServer: false, null, serverIp, serverPort);
                }
                else
                {
                    _logger.Warning("Неизвестный протокол {Protocol} для клиента", parsedModelProtocol);
                    return new ResponceIntegration { Message = "Неизвестный протокол для клиента", Result = false };
                }
            }
            else
            {
                _logger.Warning("Не указана роль: необходимо указать, является ли процесс клиентом или сервером.");
                return new ResponceIntegration { Message = "Не указана роль (клиент или сервер)", Result = false };
            }

            await Task.Delay(1000);

            _logger.Information("Настройка протокола завершена успешно для роли {Role}", isServer ? "Сервер" : "Клиент");
            return new ResponceIntegration { Message = "Настройка протокола завершена успешно", Result = true };
        }
    }
