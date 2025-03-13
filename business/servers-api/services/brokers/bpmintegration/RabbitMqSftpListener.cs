using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using servers_api.models.configurationsettings;
using servers_api.models.queues;
using servers_api.services.brokers.bpmintegration;
using IConnectionFactory = RabbitMQ.Client.IConnectionFactory;

namespace rabbit_listener
{
	public class RabbitMqSftpListener : IRabbitMqQueueListener<RabbitMqSftpListener>
	{
		private readonly IConnectionFactory _connectionFactory;
		private readonly ILogger<RabbitMqSftpListener> _logger;
		private readonly SftpConfig _config;
		private static readonly ConcurrentDictionary<string, bool> ProcessedFileHashes = new();
		private IConnection _connection;
		private IModel _channel;
		private CancellationTokenSource _cts;
		private Task _listenerTask;
		private string _pathForSave;
		private string _queueOutName;
		public RabbitMqSftpListener(IConnectionFactory connectionFactory, SftpConfig config, ILogger<RabbitMqSftpListener> logger)
		{
			_connectionFactory = connectionFactory;
			_config = config;
			_logger = logger;
		}

		public async Task StartListeningAsync(
			string queueOutName,
			CancellationToken cancellationToken,
			string pathForSave)
		{
			_pathForSave = pathForSave;
			_queueOutName = queueOutName;
			_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);


			// Подключаемся к RabbitMQ
			_connection = _connectionFactory.CreateConnection();
			_channel = _connection.CreateModel();

			var consumer = new EventingBasicConsumer(_channel);
			consumer.Received += async (model, ea) =>
			{

				var body = ea.Body.ToArray();
				var jsonMessage = Encoding.UTF8.GetString(body);

				// Десериализуем сообщение
				
					var message = JsonConvert.DeserializeObject<FileMessage>(jsonMessage);
					_logger.LogInformation($"FileName: {message.FileName}");

					byte[] fileContent = message.FileContent;
					string fileName = message.FileName;
				
				

				var filePath = Path.Combine(_pathForSave, fileName);
				await File.WriteAllBytesAsync(filePath, fileContent, _cts.Token);
				_logger.LogInformation($"Файл сохранён: {filePath}");

				// Удаляем хэш из обработанных
				string fileHash = ComputeFileHash(fileContent);
				ProcessedFileHashes.TryRemove(fileHash, out _);

				// Подтверждаем сообщение
				_channel.BasicAck(ea.DeliveryTag, false);

			};

			_channel.BasicConsume(queueOutName, false, consumer);

			// Работаем до отмены
			while (!_cts.Token.IsCancellationRequested)
			{
				await Task.Delay(1000, _cts.Token);
			}
			await Task.CompletedTask;
		}

		public void StopListening()
		{
			_logger.LogInformation("Остановка SFTP-слушателя.");
			_cts?.Cancel();
			_channel?.Close();
			_connection?.Close();
		}

		private static string ComputeFileHash(byte[] fileContent)
		{
			using var sha256 = SHA256.Create();
			byte[] hashBytes = sha256.ComputeHash(fileContent);
			return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
		}
	}
}
