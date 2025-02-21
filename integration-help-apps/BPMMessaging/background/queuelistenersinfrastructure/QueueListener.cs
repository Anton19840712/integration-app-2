using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using BPMMessaging.background.queuelistenersinfrastructure;
using System.Text;

public class QueueListener : IDisposable
{
	private readonly string _queueName;
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<QueueListener> _logger;
	private CancellationTokenSource _cancellationTokenSource;
	private Task _listeningTask;
	private DateTime _lastMessageTime;
	private static readonly TimeSpan InactivityThreshold = TimeSpan.FromSeconds(5); // Порог бездействия

	public QueueListener(string queueName, IServiceProvider serviceProvider)
	{
		_queueName = queueName;
		_serviceProvider = serviceProvider;
		_logger = _serviceProvider.GetRequiredService<ILogger<QueueListener>>();
		_lastMessageTime = DateTime.Now; // Инициализируем время последнего сообщения
	}

	public void StartListening()
	{
		_cancellationTokenSource = new CancellationTokenSource();
		_listeningTask = Task.Run(() => ListenToQueue(_cancellationTokenSource.Token));
		_logger.LogInformation($"Лисенер для {_queueName} запущен.");
	}

	private async Task ListenToQueue(CancellationToken cancellationToken)
	{
		var factory = new ConnectionFactory { HostName = "localhost" };
		using var connection = factory.CreateConnection();
		using var channel = connection.CreateModel();

		var consumer = new AsyncEventingBasicConsumer(channel);
		consumer.Received += async (_, ea) =>
		{
			try
			{
				var body = Encoding.UTF8.GetString(ea.Body.ToArray());
				using var scope = _serviceProvider.CreateScope();
				var processor = scope.ServiceProvider.GetRequiredService<IMessageProcessor>();

				await processor.ProcessMessageAsync(_queueName, body);

				// Обновляем время последнего сообщения
				_lastMessageTime = DateTime.Now;

				// Подтверждаем обработку сообщения
				channel.BasicAck(ea.DeliveryTag, false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка обработки сообщения.");
			}
		};

		channel.BasicConsume(_queueName, false, consumer);

		try
		{
			// Проверяем бездействие и создаем новый лисенер через 5 секунд
			while (!cancellationToken.IsCancellationRequested)
			{
				if (DateTime.Now - _lastMessageTime > InactivityThreshold)
				{
					_logger.LogInformation($"Лисенер для {_queueName} не получал сообщений более 5 секунд. Перезапуск...");
					StopListening(); // Останавливаем текущий лисенер
					StartListening(); // Создаем новый
					break; // Выходим из цикла
				}

				await Task.Delay(1000, cancellationToken); // Пауза 1 секунда
			}
		}
		catch (TaskCanceledException)
		{
			_logger.LogInformation($"Лисенер для {_queueName} остановлен.");
		}
	}

	public void StopListening()
	{
		_cancellationTokenSource?.Cancel();
		_cancellationTokenSource?.Dispose();
	}

	public void ProcessMessage(string message)
	{
		// Логика обработки сообщения
		_logger.LogInformation($"Обработка сообщения: {message}");
	}

	public void Dispose()
	{
		StopListening();
		_logger.LogInformation($"Лисенер для {_queueName} уничтожен.");
	}
}
