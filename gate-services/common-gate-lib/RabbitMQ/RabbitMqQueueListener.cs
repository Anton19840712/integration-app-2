
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

namespace CommonGateLib.RabbitMQ
{
    public class RabbitMqQueueListener : IRabbitMqQueueListener
    {
        private readonly ILogger<RabbitMqQueueListener> _logger;
        private readonly IConnection _connection;
        private IModel _channel;

        public RabbitMqQueueListener(IRabbitMqService rabbitMqService, ILogger<RabbitMqQueueListener> logger)
        {
            _connection = rabbitMqService.CreateConnection();
            _logger = logger;
        }

        public async Task StartListeningAsync(string queueOutName, CancellationToken stoppingToken, string pathForSave = null, Func<string, Task> onMessageReceived = null)
        {
            _channel = _connection.CreateModel();

            if (!QueueExists(queueOutName))
            {
                _logger.LogWarning("Очередь {Queue} не найдена. Создаю новую.", queueOutName);
                CreateQueue(queueOutName);
            }

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var message = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                if (onMessageReceived != null)
                    await onMessageReceived(message);
                else
                    await ProcessMessageAsync(message, queueOutName);
            };

            _channel.BasicConsume(queue: queueOutName, autoAck: true, consumer: consumer);

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Остановка слушателя очереди {Queue}.", queueOutName);
            }
        }

        protected virtual Task ProcessMessageAsync(string message, string queueName)
        {
            _logger.LogInformation("Сообщение получено из {Queue}: {Message}", queueName, message);
            return Task.CompletedTask;
        }

        private void CreateQueue(string queueName)
        {
            if (_channel == null || !_channel.IsOpen)
            {
                _channel = _connection.CreateModel();
            }

            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
        }

        public void StopListening()
        {
            _channel?.Close();
        }

        private bool QueueExists(string queueName)
        {
            try
            {
                _channel.QueueDeclarePassive(queueName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
