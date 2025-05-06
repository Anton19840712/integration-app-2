using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace listenersrabbit
{
	public class RabbitMqQueueListener : IRabbitMqQueueListener<RabbitMqQueueListener>
	{
		private readonly ILogger<RabbitMqQueueListener> _logger;
		private readonly IConnection _connection;
		private IModel _channel;

		public RabbitMqQueueListener(IRabbitMqService rabbitMqService, ILogger<RabbitMqQueueListener> logger)
		{
			_connection = rabbitMqService.CreateConnection();
			_logger = logger;
		}

		public async Task StartListeningAsync(
			string queueOutName,
			CancellationToken stoppingToken,
			string pathForSave = null,
			Func<string, Task> onMessageReceived = null) // <--- сюда можно передать обработку сообщений
		{
			_channel = _connection.CreateModel();

			const int maxAttempts = 5;
			int attempt = 0;

			// Пытаемся найти очередь 4 раза, если не нашли, то создаем на 5-й попытке
			while (attempt < maxAttempts)
			{
				attempt++;

				_logger.LogWarning("Очередь {Queue} из базы не найдена. Попытка {Attempt}/{MaxAttempts}", queueOutName, attempt, maxAttempts);

				if (attempt < maxAttempts)
				{
					// Ждем перед следующей попыткой, если попытки еще не исчерпаны
					await Task.Delay(1000, stoppingToken);
				}
				else
				{
					// На 5-й попытке создаем очередь
					_logger.LogWarning("Очередь {Queue} из базы не найдена после {MaxAttempts} попыток. Пробую создать очередь и подключиться.", queueOutName, maxAttempts);
					CreateQueue(queueOutName);
				}
			}

			// Теперь подключаемся к очереди (после того, как очередь создана)
			var consumer = new EventingBasicConsumer(_channel);

			consumer.Received += async (model, ea) =>
			{
				var message = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

				_logger.LogInformation("Получено сообщение из {Queue}: {Message}", queueOutName, message);

				if (onMessageReceived != null)
				{
					await onMessageReceived(message);
				}
				else
				{
					await ProcessMessageAsync(message, queueOutName);
				}
			};

			_logger.LogInformation("Подключен к очереди {Queue}. Ожидание сообщений...", queueOutName);
			_channel.BasicConsume(queue: queueOutName, autoAck: true, consumer: consumer);

			try
			{
				await Task.Delay(Timeout.Infinite, stoppingToken);
			}
			catch (TaskCanceledException)
			{
				_logger.LogInformation("Остановка слушателя очереди {Queue}.", queueOutName);
			}
		}

		protected virtual Task ProcessMessageAsync(string message, string queueName)
		{
			_logger.LogInformation("Обработка сообщения из {Queue}: {Message}", queueName, message);
			return Task.CompletedTask;
		}

		private void CreateQueue(string queueName)
		{
			if (_channel == null || !_channel.IsOpen)
			{
				_logger.LogWarning("Канал RabbitMQ закрыт. Пересоздаю канал перед созданием очереди...");
				_channel = _connection.CreateModel();
			}

			_logger.LogInformation("Создание очереди {Queue}", queueName);

			_channel.QueueDeclare(
				queue: queueName,
				durable: true,
				exclusive: false,
				autoDelete: false,
				arguments: null);
		}

		public void StopListening()
		{
			_logger.LogInformation("Остановка RabbitMQ слушателя...");
			_channel?.Close();
		}
	}
}
