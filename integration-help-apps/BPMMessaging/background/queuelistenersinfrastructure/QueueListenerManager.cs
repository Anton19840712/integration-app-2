using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BPMMessaging.background.queuelistenersinfrastructure
{
	public class QueueListenerManager
	{
		private readonly ConcurrentDictionary<string, QueueListener> _listeners = new();
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<QueueListenerManager> _logger;

		public QueueListenerManager(IServiceProvider serviceProvider, ILogger<QueueListenerManager> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		public void StartListener(string queueName)
		{
			// Если лисенер для очереди уже существует, просто возвращаем
			if (_listeners.ContainsKey(queueName))
			{
				_logger.LogInformation($"Лисенер для {queueName} уже запущен.");
				return;
			}

			try
			{
				// Создаем и запускаем новый лисенер
				var listener = new QueueListener(queueName, _serviceProvider.CreateScope().ServiceProvider);
				_listeners[queueName] = listener;

				// Запускаем прослушивание
				listener.StartListening();

				_logger.LogInformation($"Лисенер для {queueName} запущен.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при запуске лисенера для {queueName}.");
			}
		}

		public void StopListener(string queueName)
		{
			if (_listeners.TryRemove(queueName, out var listener))
			{
				listener.StopListening();
				_logger.LogInformation($"Лисенер для {queueName} остановлен.");
			}
			else
			{
				_logger.LogWarning($"Не удалось найти лисенер для {queueName}.");
			}
		}
	}
}
