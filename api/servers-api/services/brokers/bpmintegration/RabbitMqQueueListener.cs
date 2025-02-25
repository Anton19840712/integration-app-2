using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using servers_api.repositories;

namespace servers_api.services.brokers.bpmintegration
{
	public class RabbitMqQueueListener : IRabbitMqQueueListener
	{
		private readonly ILogger<RabbitMqQueueListener> _logger;
		private readonly IConnectionFactory _connectionFactory;
		private readonly QueuesRepository _queuesRepository;

		private IConnection _connection;
		private IModel _channel;
		private string _queueOutName;

		public RabbitMqQueueListener(
			IConnectionFactory connectionFactory,
			ILogger<RabbitMqQueueListener> logger,
			QueuesRepository queuesRepositoryy)
		{
			_connectionFactory = connectionFactory;
			_logger = logger;
			_queuesRepository = queuesRepositoryy;
		}

		public async Task StartListeningAsync(
			string queueOutName,
			CancellationToken stoppingToken)
		{
			_queueOutName = queueOutName;
			_connection = _connectionFactory.CreateConnection();
			_channel = _connection.CreateModel();

			while (!QueueExists(_channel, _queueOutName))
			{
				_logger.LogWarning("Очередь {Queue} еще не создана. Ожидание...", _queueOutName);
				await Task.Delay(1000, stoppingToken);
			}

			var consumer = new EventingBasicConsumer(_channel);
			consumer.Received += async (model, ea) => await HandleMessageAsync(ea);

			_channel.BasicConsume(queue: _queueOutName, autoAck: true, consumer: consumer);
			_logger.LogInformation("Слушатель очереди {Queue} запущен", _queueOutName);
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

		private async Task HandleMessageAsync(BasicDeliverEventArgs ea)
		{
			var message = Encoding.UTF8.GetString(ea.Body.ToArray());
			_logger.LogInformation("Получено сообщение из очереди {Queue}: {Message}", _queueOutName, message);
			await Task.Delay(0);
		}

		public void StopListening()
		{
			_channel?.Close();
			_connection?.Close();
			_logger.LogInformation("Слушатель {Queue} остановлен", _queueOutName);
		}
	}
}
