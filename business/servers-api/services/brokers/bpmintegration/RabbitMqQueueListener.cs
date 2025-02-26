using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace servers_api.services.brokers.bpmintegration
{
	public class RabbitMqQueueListener : IRabbitMqQueueListener
	{
		private readonly ILogger<RabbitMqQueueListener> _logger;
		private readonly IConnectionFactory _connectionFactory;

		public RabbitMqQueueListener(
			IConnectionFactory connectionFactory,
			ILogger<RabbitMqQueueListener> logger)
		{
			_connectionFactory = connectionFactory;
			_logger = logger;
		}

		public async Task StartListeningAsync(string queueOutName, CancellationToken stoppingToken)
		{
			using var connection = _connectionFactory.CreateConnection();
			using var channel = connection.CreateModel();

			while (!QueueExists(channel, queueOutName))
			{
				_logger.LogWarning("Очередь {Queue} еще не создана. Ожидание...", queueOutName);
				await Task.Delay(1000, stoppingToken);
			}

			var messages = new List<string>();
			var messageReceived = false; // Флаг, отслеживающий поступление сообщений

			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				var message = Encoding.UTF8.GetString(ea.Body.ToArray());
				messages.Add(message);
				messageReceived = true; // Устанавливаем флаг при получении сообщения
			};

			_logger.LogInformation("Подключаемся к очереди {Queue}", queueOutName);
			channel.BasicConsume(queue: queueOutName, autoAck: true, consumer: consumer);

			await Task.Delay(5000, stoppingToken);

			if (messages.Count > 0)
			{
				foreach (var message in messages)
				{
					_logger.LogInformation("Получено сообщение: {Message}", message);
				}
			}
			else
			{
				_logger.LogWarning("В очереди {Queue} нет новых сообщений.", queueOutName);
			}

			messages.Clear();
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

		public void StopListening()
		{
			throw new NotImplementedException();
		}
	}
}
