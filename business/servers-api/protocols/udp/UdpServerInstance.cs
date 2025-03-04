using System.Net;
using System.Net.Sockets;
using servers_api.models.internallayer.instance;
using servers_api.validation;
using servers_api.messaging.sending;
using servers_api.models.response;
using servers_api.factory;
using System.Text;

namespace servers_api.protocols.udp
{
	/// <summary>
	/// UDP сервер, который продолжает отправлять сообщения после возврата ResponseIntegration.
	/// </summary>
	public class UdpServerInstance : IUpServer
	{
		private readonly IMessageSender _messageSender;
		private readonly ILogger<UdpServerInstance> _logger;
		private readonly IServerInstanceFluentValidator _validator;

		public UdpServerInstance(
			IMessageSender messageSender,
			IServerInstanceFluentValidator validator,
			ILogger<UdpServerInstance> logger)
		{
			_messageSender = messageSender;
			_validator = validator;
			_logger = logger;
		}

		public async Task<ResponseIntegration> UpServerAsync(
			ServerInstanceModel instanceModel,
			CancellationToken cancellationToken = default)
		{
			var validationResponse = _validator.Validate(instanceModel);
			if (validationResponse != null)
				return validationResponse;

			var endpoint = new IPEndPoint(IPAddress.Parse(instanceModel.Host), instanceModel.Port);
			var udpServer = new UdpClient(endpoint);

			_logger.LogInformation("UDP сервер запущен на {Host}:{Port}", instanceModel.Host, instanceModel.Port);

			try
			{
				// Логируем информацию о запуске сервера
				_logger.LogInformation("Сервер ожидает подключения клиента...");

				// Цикл для постоянного приема сообщений
				while (true)
				{
					try
					{
						var receivedResult = await udpServer.ReceiveAsync(cancellationToken);
						_logger.LogInformation("Получены данные от клиента {RemoteEndPoint}: {Data}",
							receivedResult.RemoteEndPoint, Encoding.UTF8.GetString(receivedResult.Buffer));

						// Ответ на полученные данные
						var responseData = Encoding.UTF8.GetBytes("Ответ от сервера: сообщение получено");
						await udpServer.SendAsync(responseData, responseData.Length, receivedResult.RemoteEndPoint);
						_logger.LogInformation("Ответ отправлен клиенту {RemoteEndPoint}", receivedResult.RemoteEndPoint);
					}
					catch (OperationCanceledException)
					{
						// Игнорируем исключение отмены, продолжаем ожидание подключений
						_logger.LogInformation("Ожидание клиента продолжено...");
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Ошибка при приеме сообщения.");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка запуска UDP-сервера.");
				return new ResponseIntegration
				{
					Message = "Ошибка запуска UDP-сервера.",
					Result = false
				};
			}
		}
	}
}
