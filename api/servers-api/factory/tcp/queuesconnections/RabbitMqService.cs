namespace servers_api.factory.tcp.queuesconnections
{
	using RabbitMQ.Client;
	using RabbitMQ.Client.Events;
	using RabbitMQ.Client.Exceptions;
	using System.Text;

	public class RabbitMqService : IRabbitMqService
	{
		private readonly ConnectionFactory _factory;
		private ILogger<RabbitMqService> _logger;
		private IConnection _persistentConnection;

		public RabbitMqService(ILogger<RabbitMqService> logger)
		{
			_factory = new ConnectionFactory
			{
				HostName = "localhost",
				Port = 5672,
				UserName = "guest",
				Password = "guest"
			};
			_logger = logger;
		}

		// Свойство для получения постоянного соединения
		private IConnection PersistentConnection
		{
			get
			{
				if (_persistentConnection != null)
					return _persistentConnection;

				var attempt = 0;
				var maxAttempts = 5;  // Количество попыток подключения
				var delayMs = 3000;   // Интервал между попытками (3 сек)

				while (attempt < maxAttempts)
				{
					try
					{
						_persistentConnection = _factory.CreateConnection();
						return _persistentConnection;
					}
					catch (BrokerUnreachableException ex)
					{
						attempt++;
						_logger.LogWarning($"Попытка {attempt}/{maxAttempts}: не удалось подключиться к RabbitMQ ({ex.Message}).");

						if (attempt == maxAttempts)
						{
							_logger.LogError("Исчерпаны все попытки подключения к RabbitMQ.");
							throw;
						}

						Thread.Sleep(delayMs);
					}
				}

				throw new InvalidOperationException("Не удалось установить соединение с RabbitMQ.");
			}
		}

		public ILogger<RabbitMqService> Logger { get; }


		// Метод для публикации сообщений
		public void PublishMessage(string queueName, string message)
		{
			using var channel = PersistentConnection.CreateModel();

			channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

			var body = Encoding.UTF8.GetBytes(message);
			channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
		}

		// Метод для возврата существующего соединения
		public IConnection CreateConnection()
		{
			return PersistentConnection;
		}

		// Ожидание ответа с таймаутом, если ответ не получен, соединение прекращается
		public async Task<string> WaitForResponse(string queueName, int timeoutMilliseconds = 15000)
		{
			using var channel = PersistentConnection.CreateModel();

			channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

			var consumer = new EventingBasicConsumer(channel);
			var completionSource = new TaskCompletionSource<string>();

			consumer.Received += (model, ea) =>
			{
				var response = Encoding.UTF8.GetString(ea.Body.ToArray());
				completionSource.SetResult(response);
			};

			channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

			var completedTask = await Task.WhenAny(completionSource.Task, Task.Delay(timeoutMilliseconds));
			return completedTask == completionSource.Task ? completionSource.Task.Result : null;
		}
	}
}
