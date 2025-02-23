using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using servers_api.models.response;

namespace servers_api.services.brokers.bpmintegration
{

	/// <summary>
	/// RabbitMqQueueListener с использованием промежуточной конкурентной коллекции с накоплением сообщений и последующей выдачей на сервер.
	/// Можно было передавать промежуточно поточно с использованием различных решений,
	/// но это оказалось наиболее коротким. В зависимости от поведения системы возможно
	/// добавление раличных реализаций: networkstream, kafka, database and outbox. Но они на данный mvp момент излишни.
	/// </summary>
	public class RabbitMqQueueListener(
		IConnectionFactory connectionFactory,
		ILogger<RabbitMqQueueListener> logger) : IRabbitMqQueueListener
	{
		private IConnection _connection;
		private IModel _channel;
		private string _queueName;
		private readonly ConcurrentQueue<ResponseIntegration> _collectedMessages = new();

		public async Task StartListeningAsync(string queueName, CancellationToken stoppingToken)
		{
			_queueName = queueName;

			try
			{
				_connection = connectionFactory.CreateConnection();
				_channel = _connection.CreateModel();

				// Ждём, пока очередь не появится
				while (!QueueExists(_channel, _queueName))
				{
					logger.LogWarning("Очередь {Queue} еще не создана. Ожидание...", _queueName);
					await Task.Delay(1000, stoppingToken);
				}

				var consumer = new EventingBasicConsumer(_channel);
				consumer.Received += async (model, ea) => await HandleMessageAsync(ea);

				_channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
				logger.LogInformation("Слушатель очереди {Queue} запущен", _queueName);

				while (!stoppingToken.IsCancellationRequested)
				{
					await Task.Delay(5000, stoppingToken); // Ожидание перед обработкой следующей партии
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Ошибка при прослушивании {Queue}", _queueName);
			}
			finally
			{
				StopListening();
			}
		}

		private bool QueueExists(IModel channel, string queueName)
		{
			try
			{
				using var tempChannel = _connection.CreateModel(); // Создаём новый канал для проверки
				tempChannel.QueueDeclarePassive(queueName);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private Task HandleMessageAsync(BasicDeliverEventArgs ea)
		{
			var message = Encoding.UTF8.GetString(ea.Body.ToArray());

			logger.LogInformation("Получено сообщение из очереди {Queue}: {Message}", _queueName, message);

			_collectedMessages.Enqueue(new ResponseIntegration { Message = message, Result = true });

			return Task.CompletedTask;
		}


		public void StopListening()
		{
			_channel?.Close();
			_connection?.Close();
			logger.LogInformation("Слушатель {Queue} остановлен", _queueName);
		}

		public Task<List<ResponseIntegration>> GetCollectedMessagesAsync(CancellationToken stoppingToken)
		{
			var messagesList = new List<ResponseIntegration>();

			while (_collectedMessages.TryDequeue(out var message))
			{
				messagesList.Add(message);
			}
			return Task.FromResult(messagesList);
		}
	}
}