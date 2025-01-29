using RabbitMQ.Client;
using servers_api.models;
using servers_api.Models;

namespace servers_api.Services.Brokers
{
	/// <summary>
	/// Менеджер занимается созданием очередей в сетевой шине.
	/// Когда настраивается интеграция, данный класс создает очереди в сетевой шине под нее отдельно.
	/// </summary>
	public class RabbitMqQueueManager : IRabbitMqQueueManager
	{
		private readonly ILogger<RabbitMqQueueManager> _logger;
		private readonly IConnectionFactory _connectionFactory;
		private readonly string _hostname = "localhost";
		private readonly int _port = 5672;
		private readonly string _username = "guest";
		private readonly string _password = "guest";

		public RabbitMqQueueManager(ILogger<RabbitMqQueueManager> logger)
		{
			_logger = logger;
			_connectionFactory = new ConnectionFactory
			{
				HostName = _hostname,
				Port = _port,
				UserName = _username,
				Password = _password
			};
		}

		public async Task<ResponceIntegration> CreateQueues(string queueIn, string queueOut)
		{
			_logger.LogInformation("Попытка создания подключения к RabbitMQ на {Host}:{Port} с пользователем {User}", _hostname, _port, _username);

			try
			{
				using (var connection = await Task.Run(() => _connectionFactory.CreateConnection()))
				using (var channel = connection.CreateModel())
				{
					_logger.LogInformation("Соединение с RabbitMQ установлено успешно.");

					// Объявление входной очереди
					channel.QueueDeclare(queue: queueIn,
										 durable: false,
										 exclusive: false,
										 autoDelete: false,
										 arguments: null);
					_logger.LogInformation("Очередь {QueueIn} успешно создана.", queueIn);

					// Объявление выходной очереди
					channel.QueueDeclare(queue: queueOut,
										 durable: false,
										 exclusive: false,
										 autoDelete: false,
										 arguments: null);
					_logger.LogInformation("Очередь {QueueOut} успешно создана.", queueOut);

					return new ResponceQueuesIntegration
					{
						Message = $"Очереди '{queueIn}' и '{queueOut}' успешно созданы.",
						Result = true,
						OutQueue = queueOut
					};
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при создании очередей {QueueIn} и {QueueOut}: {Message}", queueIn, queueOut, ex.Message);
				return new ResponceQueuesIntegration
				{
					Message = $"Ошибка при создании очередей '{queueIn}' и '{queueOut}': {ex.Message}",
					Result = false
				};
			}
		}
	} 
}
