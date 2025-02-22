using RabbitMQ.Client;
using servers_api.models.response;

namespace servers_api.services.brokers.tcprest
{
	public class RabbitQueuesCreator(ILogger<RabbitQueuesCreator> logger, IConnectionFactory connectionFactory) : IRabbitQueuesCreator
	{
		public async Task<ResponseIntegration> CreateQueuesAsync(
			string queueIn,
			string queueOut,
			CancellationToken stoppingToken)
		{
			logger.LogInformation("Создание очередей {QueueIn} и {QueueOut}", queueIn, queueOut);

			try
			{
				return await Task.Run(() =>
				{
					using var connection = connectionFactory.CreateConnection();
					using var channel = connection.CreateModel();

					DeclareQueue(channel, queueIn);
					DeclareQueue(channel, queueOut);

					return new ResponseQueuesIntegration
					{
						Message = $"Очереди '{queueIn}' и '{queueOut}' успешно созданы.",
						Result = true,
						OutQueue = queueOut
					};
				});
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Ошибка при создании очередей {QueueIn} и {QueueOut}", queueIn, queueOut);
				return new ResponseQueuesIntegration { Message = ex.Message, Result = false };
			}
		}
		private void DeclareQueue(IModel channel, string queue)
		{
			channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);
			logger.LogInformation("Очередь {Queue} создана.", queue);
		}
	}
}
