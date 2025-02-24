using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using BPMMessaging.models.dtos;

public class RabbitMqQueueListener
{
	private readonly ILogger<RabbitMqQueueListener> _logger;
	private readonly IConnectionFactory _connectionFactory;
	private IConnection _connection;
	private IModel _channel;
	private string _queueName;
	private readonly ConcurrentQueue<ResponseIntegration> _collectedMessages = new();

	public RabbitMqQueueListener(IConnectionFactory connectionFactory, ILogger<RabbitMqQueueListener> logger)
	{
		_connectionFactory = connectionFactory;
		_logger = logger;
	}

	public async Task StartListeningAsync(string queueName, CancellationToken stoppingToken)
	{
		_queueName = queueName;
		_connection = _connectionFactory.CreateConnection();
		_channel = _connection.CreateModel();

		while (!QueueExists(_channel, _queueName))
		{
			_logger.LogWarning("Очередь {Queue} еще не создана. Ожидание...", _queueName);
			await Task.Delay(1000, stoppingToken);
		}

		var consumer = new EventingBasicConsumer(_channel);
		consumer.Received += async (model, ea) => await HandleMessageAsync(ea);

		_channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
		_logger.LogInformation("Слушатель очереди {Queue} запущен", _queueName);
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
		_logger.LogInformation("Получено сообщение из очереди {Queue}: {Message}", _queueName, message);
		_collectedMessages.Enqueue(new ResponseIntegration { Message = message, Result = true });
		await Task.CompletedTask;
	}

	public List<ResponseIntegration> GetCollectedMessages()
	{
		var messagesList = new List<ResponseIntegration>();
		while (_collectedMessages.TryDequeue(out var message))
		{
			messagesList.Add(message);
		}
		return messagesList;
	}

	public void StopListening()
	{
		_channel?.Close();
		_connection?.Close();
		_logger.LogInformation("Слушатель {Queue} остановлен", _queueName);
	}
}
