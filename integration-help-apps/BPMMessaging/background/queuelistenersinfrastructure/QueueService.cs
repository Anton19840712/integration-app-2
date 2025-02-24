using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Options;
using BPMMessaging.models.settings;

namespace BPMMessaging.background.queuelistenersinfrastructure
{
	public class QueueService : IQueueService
	{
		private readonly ILogger<QueueService> _logger;
		private readonly ConcurrentDictionary<string, CancellationTokenSource> _listeners = new();
		private readonly ConcurrentDictionary<string, IModel> _channels = new();
		private readonly IConnection _connection;

		public QueueService(
			ILogger<QueueService> logger,
			IOptions<RabbitMqSettings> rabbitMqSettings)
		{
			_logger = logger;

			var factory = new ConnectionFactory
			{
				HostName = rabbitMqSettings.Value.HostName,
				Port = rabbitMqSettings.Value.Port,
				UserName = rabbitMqSettings.Value.UserName,
				Password = rabbitMqSettings.Value.Password
			};

			_connection = factory.CreateConnection();
			_logger.LogInformation($"Подключение к RabbitMQ на {rabbitMqSettings.Value.HostName}:{rabbitMqSettings.Value.Port}");
		}

		public void StartListener(string queueName)
		{
			if (_listeners.ContainsKey(queueName))
			{
				_logger.LogInformation($"Лисенер для {queueName} уже запущен.");
				return;
			}

			var cts = new CancellationTokenSource();
			_listeners[queueName] = cts;

			Task.Run(() => ListenToQueue(queueName, cts.Token), cts.Token);
			_logger.LogInformation($"Лисенер для {queueName} запущен.");
		}

		private async Task ListenToQueue(string queueName, CancellationToken cancellationToken)
		{
			using var channel = _connection.CreateModel();
			_channels[queueName] = channel;

			var consumer = new AsyncEventingBasicConsumer(channel);
			consumer.Received += async (_, ea) =>
			{
				var body = Encoding.UTF8.GetString(ea.Body.ToArray());
				_logger.LogInformation($"Получено сообщение: {body}");

				try
				{
					// Ваш код для обработки сообщения, например:
					// await ProcessMessage(body);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Ошибка обработки сообщения.");
				}
				finally
				{
					// В любом случае подтверждаем получение сообщения
					channel.BasicAck(ea.DeliveryTag, false);
				}
			};

			channel.BasicConsume(queueName, false, consumer);
			_logger.LogInformation($"Начата обработка сообщений из {queueName}");

			// Ожидаем, пока слушатель не будет отменен
			await Task.Delay(Timeout.Infinite, cancellationToken);
		}

	}
}
