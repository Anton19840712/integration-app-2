using System.Collections.Concurrent;
using System.Security.Cryptography;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Renci.SshNet;
using servers_api.models.configurationsettings;
using servers_api.services.brokers.bpmintegration;

namespace rabbit_listener
{
	public class FileMessage
	{
		public byte[] FileContent { get; set; }
		public string FileExtension { get; set; }
	}

	public class RabbitMqSftpListener : IRabbitMqQueueListener
	{
		private readonly ILogger<RabbitMqSftpListener> _logger;
		private readonly SftpConfig _config;
		private static readonly ConcurrentDictionary<string, bool> ProcessedFileHashes = new();

		public RabbitMqSftpListener(
			SftpConfig config,
			IConnectionFactory connectionFactory,
			ILogger<RabbitMqSftpListener> logger)
			: base(connectionFactory, logger)
		{
			_config = config;
			_logger = logger;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			return StartListeningAsync("sftp_queue", cancellationToken);
		}

		protected override async Task ProcessMessageAsync(string message, string queueName)
		{
			try
			{
				var fileMessage = JsonConvert.DeserializeObject<FileMessage>(message);
				if (fileMessage == null)
				{
					_logger.LogWarning("Получено некорректное сообщение.");
					return;
				}

				byte[] fileContent = fileMessage.FileContent;
				string fileExtension = fileMessage.FileExtension;
				string filePath = Path.Combine("C:/Downloads3", $"file_{Guid.NewGuid()}{fileExtension}");

				await File.WriteAllBytesAsync(filePath, fileContent);
				UploadToSftp(filePath);

				string fileHash = ComputeFileHash(fileContent);
				ProcessedFileHashes.TryRemove(fileHash, out _);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка обработки файла.");
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			StopListening();
			return Task.CompletedTask;
		}

		private static string ComputeFileHash(byte[] fileContent)
		{
			using var sha256 = SHA256.Create();
			return BitConverter.ToString(sha256.ComputeHash(fileContent)).Replace("-", "").ToLower();
		}

		private void UploadToSftp(string filePath)
		{
			try
			{
				using var sftpClient = new SftpClient(_config.Host, _config.Port, _config.UserName, _config.Password);
				sftpClient.Connect();
				using var fileStream = File.OpenRead(filePath);
				var remotePath = Path.Combine("/remote/path", Path.GetFileName(filePath));
				sftpClient.UploadFile(fileStream, remotePath);
				sftpClient.Disconnect();

				_logger.LogInformation("Файл успешно загружен на SFTP сервер.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при загрузке файла на SFTP.");
			}
		}

		public Task StartListeningAsync(string queueOutName, CancellationToken stoppingToken, string pathForSave = null)
		{
			throw new NotImplementedException();
		}

		public void StopListening()
		{
			throw new NotImplementedException();
		}
	}
}
