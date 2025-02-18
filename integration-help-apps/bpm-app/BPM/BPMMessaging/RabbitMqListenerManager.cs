using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BPMMessaging
{
	public class RabbitMqListenerManager
	{
		private readonly IConnectionFactory _connectionFactory;
		private readonly QueueConfigRepository _configRepository;
		private readonly IMongoDatabase _mongoDatabase;
		private readonly Dictionary<string, Task> _listeners = new();

		public RabbitMqListenerManager(
			IConnectionFactory connectionFactory,
			QueueConfigRepository configRepository,
			IMongoDatabase mongoDatabase)
		{
			_connectionFactory = connectionFactory;
			_configRepository = configRepository;
			_mongoDatabase = mongoDatabase;
		}

		public async Task StartListenersAsync()
		{
			var configs = await _configRepository.GetActiveQueuesAsync();

			foreach (var config in configs)
			{
				if (!_listeners.ContainsKey(config.IncomingQueue))
				{
					var task = Task.Run(() => StartListening(config));
					_listeners[config.IncomingQueue] = task;
				}
			}
		}

		private void StartListening(QueueConfig config)
		{
			using var connection = _connectionFactory.CreateConnection();
			using var channel = connection.CreateModel();
			channel.QueueDeclare(config.IncomingQueue, true, false, false, null);

			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += async (sender, e) =>
			{
				var message = Encoding.UTF8.GetString(e.Body.ToArray());
				await SaveMessageToMongo(config, message);
			};

			channel.BasicConsume(config.IncomingQueue, true, consumer);
			Console.WriteLine($"Listening on {config.IncomingQueue}...");
		}

		private async Task SaveMessageToMongo(QueueConfig config, string message)
		{
			var collection = _mongoDatabase.GetCollection<BsonDocument>("messages");
			var document = new BsonDocument
		{
			{ "queue", config.IncomingQueue },
			{ "message", BsonDocument.Parse(message) },
			{ "timestamp", DateTime.UtcNow }
		};

			await collection.InsertOneAsync(document);
		}
	}
}
