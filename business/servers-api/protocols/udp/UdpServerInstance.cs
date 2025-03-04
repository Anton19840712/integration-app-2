using System.Net.Sockets;
using System.Net;
using servers_api.models.internallayer.instance;
using servers_api.validation;
using servers_api.messaging.sending;
using servers_api.models.response;
using servers_api.factory;

namespace servers_api.protocols.udp;

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
		using var udpServer = new UdpClient(endpoint);

		_logger.LogInformation("UDP сервер запущен на {Host}:{Port}", instanceModel.Host, instanceModel.Port);

		_ = Task.Run(async () =>
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					var receivedResult = await udpServer.ReceiveAsync(cancellationToken);
					string message = System.Text.Encoding.UTF8.GetString(receivedResult.Buffer);
					_logger.LogInformation("Получено сообщение: {Message}", message);

					// Ответ клиенту (если необходимо)
					byte[] responseData = System.Text.Encoding.UTF8.GetBytes("Сообщение принято.");
					await udpServer.SendAsync(responseData, responseData.Length, receivedResult.RemoteEndPoint);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Ошибка при обработке входящего UDP сообщения.");
				}
			}
		}, cancellationToken);

		return new ResponseIntegration
		{
			Message = "UDP сервер запущен.",
			Result = true
		};
	}
}
