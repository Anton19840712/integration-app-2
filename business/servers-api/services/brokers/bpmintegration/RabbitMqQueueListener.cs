using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace servers_api.services.brokers.bpmintegration
{
	public class RabbitMqQueueListener : IRabbitMqQueueListener
	{
		private readonly ILogger<RabbitMqQueueListener> _logger;
		private readonly IConnectionFactory _connectionFactory;
		private IConnection _connection;
		private IModel _channel;

		public RabbitMqQueueListener(IConnectionFactory connectionFactory, ILogger<RabbitMqQueueListener> logger)
		{
			_connectionFactory = connectionFactory;
			_logger = logger;
		}

		public async Task StartListeningAsync(string queueOutName, CancellationToken stoppingToken)
		{
			_connection = _connectionFactory.CreateConnection();
			_channel = _connection.CreateModel();

			while (!QueueExists(_channel, queueOutName))
			{
				_logger.LogWarning("Очередь {Queue} не найдена. Ожидание...", queueOutName);
				await Task.Delay(1000, stoppingToken);
			}

			var consumer = new EventingBasicConsumer(_channel);
			consumer.Received += async (model, ea) =>
			{
				var message = Encoding.UTF8.GetString(ea.Body.ToArray());
				_logger.LogInformation("Получено сообщение из {Queue}: {Message}", queueOutName, message);

				await ProcessMessageAsync(message, queueOutName);
			};

			_logger.LogInformation("Подключен к очереди {Queue}. Ожидание сообщений...", queueOutName);
			_channel.BasicConsume(queue: queueOutName, autoAck: true, consumer: consumer);

			// Держим процесс активным, пока не получен сигнал отмены
			try
			{
				await Task.Delay(Timeout.Infinite, stoppingToken);
			}
			catch (TaskCanceledException)
			{
				_logger.LogInformation("Остановка слушателя очереди {Queue}.", queueOutName);
			}
		}

		private bool QueueExists(IModel channel, string queueName)
		{
			try
			{
				channel.QueueDeclarePassive(queueName);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private Task ProcessMessageAsync(string message, string queueName)
		{
			// Логика обработки сообщений
			_logger.LogInformation("Обработка сообщения из {Queue}: {Message}", queueName, message);
			return Task.CompletedTask;
		}

		public void StopListening()
		{
			_logger.LogInformation("Остановка RabbitMQ слушателя...");
			_channel?.Close();
			_connection?.Close();
		}
	}
}
