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
			// Создаем новое подключение и канал для каждой очереди.
			using var connection = _connectionFactory.CreateConnection();
			using var channel = connection.CreateModel();

			// Ожидаем появления очереди.
			while (!QueueExists(channel, queueOutName))
			{
				_logger.LogWarning("Очередь {Queue} еще не создана. Ожидание...", queueOutName);
				await Task.Delay(1000, stoppingToken);
			}

			// Создаем список для хранения сообщений
			var messages = new List<string>();

			// Создаем потребителя для этой очереди.
			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				// Чтение сообщения из тела события
				var message = Encoding.UTF8.GetString(ea.Body.ToArray());

				// Добавляем сообщение в коллекцию
				messages.Add(message);
			};

			// Логируем начало подключения к очереди
			_logger.LogInformation("Подключаемся к очереди {Queue}", queueOutName);

			// Подключаемся к очереди.
			channel.BasicConsume(queue: queueOutName, autoAck: true, consumer: consumer);

			// Ожидаем получения сообщений
			await Task.Delay(5000, stoppingToken); // Ожидаем 5 секунд для получения сообщений (можно настроить)

			// После того как сообщения собраны, выводим их на печать
			foreach (var message in messages)
			{
				_logger.LogInformation("Получено сообщение: {Message}", message);
			}

			// Очистка коллекции после вывода сообщений
			messages.Clear();
		}


		// Проверка существования очереди.
		private bool QueueExists(IModel channel, string queueName)
		{
			try
			{
				channel.QueueDeclarePassive(queueName); // Проверка на существование очереди
				return true;
			}
			catch
			{
				return false; // Если очередь не существует, возвращаем false
			}
		}

		public void StopListening()
		{
			// Реализуйте остановку слушателя, если это необходимо
			throw new NotImplementedException();
		}
	}
}
