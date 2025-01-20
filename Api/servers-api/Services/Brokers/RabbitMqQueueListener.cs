using System.Text;
using System.Threading.Channels;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using servers_api.Models;
using ILogger = Serilog.ILogger;

namespace servers_api.Services.Brokers;

/// <summary>
/// Не совсем понятно, возможно, что после обучения стоит удалять очереди, потому что обучение произошло как таковое.
/// </summary>
public class RabbitMqQueueListener : IRabbitMqQueueListener
{
	private readonly IConnectionFactory _connectionFactory;
	private readonly ILogger _logger;
	private IConnection _connection;
	private IModel _channel;
	private string _queueName;
	private readonly Channel<ResponceIntegration> _responseChannel;

	public RabbitMqQueueListener(IConnectionFactory connectionFactory, ILogger logger)
	{
		_connectionFactory = connectionFactory;
		_logger = logger;
		_responseChannel = Channel.CreateUnbounded<ResponceIntegration>();
	}

	public async Task<ResponceIntegration> StartListeningAsync(string queueName, CancellationToken stoppingToken)
	{
		_queueName = queueName;

		try
		{
			_connection = _connectionFactory.CreateConnection();
			_channel = _connection.CreateModel();

			var consumer = new EventingBasicConsumer(_channel);

			consumer.Received += async (model, ea) =>
			{
				var body = ea.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);

				// Десериализация основного сообщения
				var mainMessage = JsonConvert.DeserializeObject<OutMessage>(message);
				if (mainMessage != null)
				{
					string jsonString = mainMessage.IncomingModel.ToString(Formatting.None);

					// Логирование в одном сообщении
					_logger.Information(
						"Получено сообщение из {Queue}:\nId: {Id} \nInQueueName: {InQueueName} \nOutQueueName: {OutQueueName} \nIncomingModel: {IncomingModel}",
						_queueName,
						mainMessage.Id,
						mainMessage.InQueueName,
						mainMessage.OutQueueName,
						jsonString);

					// Добавляем результат в канал
					await _responseChannel.Writer.WriteAsync(new ResponceIntegration
					{
						Message = message,
						Result = true
					}, stoppingToken);
				}
				else
				{
					_logger.Warning("Получено пустое или некорректное сообщение из {Queue}", _queueName);
				}
			};

			_channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
			_logger.Information("Слушатель очереди {Queue} запущен и ожидает сообщений...", _queueName);

			// Чтение из канала и возврат результата при получении сообщения
			return await _responseChannel.Reader.ReadAsync(stoppingToken);
		}
		catch (Exception ex)
		{
			_logger.Error(ex, "Ошибка при прослушивании очереди {Queue}", _queueName);
			return new ResponceIntegration
			{
				Message = $"Ошибка при прослушивании очереди {_queueName}: {ex.Message}",
				Result = false
			};
		}
		finally
		{
			StopListening();
		}
	}
	public void StopListening()
	{
		_channel?.Close();
		_connection?.Close();
		_logger.Information("Слушатель очереди {Queue} остановлен", _queueName);
	}
}