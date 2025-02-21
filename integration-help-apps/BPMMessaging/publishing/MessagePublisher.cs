using System.Text;
using BPMMessaging.models.dtos;
using BPMMessaging.models.settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace BPMMessaging.publishing
{
	public class MessagePublisher : IMessagePublisher
	{
		private readonly ILogger<MessagePublisher> _logger;
		private readonly ConnectionFactory _connectionFactory;

		public MessagePublisher(
			ILogger<MessagePublisher> logger,
			IOptions<RabbitMqSettings> rabbitMqOptions)
		{
			_logger = logger;
			var rabbitMqSettings = rabbitMqOptions.Value;

			_connectionFactory = new ConnectionFactory
			{
				HostName = rabbitMqSettings.HostName,
				Port = rabbitMqSettings.Port,
				UserName = rabbitMqSettings.UserName,
				Password = rabbitMqSettings.Password
			};
		}

		public async Task PublishAsync(string queueName, OutboxMessage payload)
		{
			try
			{
				await Task.Run(() =>
				{
					using var connection = _connectionFactory.CreateConnection();
					using var channel = connection.CreateModel();

					var jsonString = JsonConvert.SerializeObject(payload);
					var body = Encoding.UTF8.GetBytes(jsonString);

					channel.BasicPublish(
						exchange: "",
						routingKey: queueName,
						basicProperties: null,
						body: body
					);

					_logger.LogInformation($"Сообщение опубликовано в очередь {queueName}: {jsonString}");
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при публикации сообщения в очередь {QueueName}: {Payload}", queueName, payload);
			}
		}
	}
}
